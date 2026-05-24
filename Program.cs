using Serilog;
using Serilog.Sinks.Elasticsearch;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Data.SqlClient;
using New_folder;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "dotnet-logs-{0:yyyy.MM.dd}"
    })
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add the Kafka consumer as a hosted service
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

// ----- Admin Client to create the topic on startup -----
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminConfig = new AdminClientConfig { BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092" };

    try
    {
        using var adminClient = new AdminClientBuilder(adminConfig).Build();
        
        // Check if topic exists
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topicExists = metadata.Topics.Any(t => t.Topic == "csv-data");

        if (!topicExists)
        {
            Console.WriteLine("Topic 'csv-data' does not exist. Creating it now...");
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification { Name = "csv-data", ReplicationFactor = 1, NumPartitions = 1 }
            });
            Console.WriteLine("Topic 'csv-data' created successfully.");
        }
        else
        {
            Console.WriteLine("Topic 'csv-data' already exists.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while ensuring the topic exists: {ex.Message}");
    }
}
// -------------------------------------------------------


app.MapGet("/", () => "Hello World! This is the API for processing CSV files with Kafka.");

// This endpoint now produces messages to a Kafka topic
app.MapPost("/upload", async (IFormFile file, IConfiguration config) =>
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded.");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092"
        };

        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        using var reader = new StreamReader(file.OpenReadStream());

        // Skip header row
        var header = await reader.ReadLineAsync();
        if (header == null)
        {
            return Results.BadRequest("Empty file or invalid CSV format.");
        }

        var lineCount = 0;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                // Produce the CSV line to the 'csv-data' topic
                producer.Produce("csv-data", new Message<Null, string> { Value = line });
                lineCount++;
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                // Optionally, return an error to the client
                return Results.Problem($"Failed to produce message to Kafka: {e.Error.Reason}");
            }
        }

        producer.Flush(TimeSpan.FromSeconds(10));
        return Results.Ok($"{lineCount} lines from the CSV have been queued for processing via Kafka.");
    })
    .DisableAntiforgery();

app.MapPost("/customer", async (New_folder.Models.CustomerDto customer, IConfiguration config) =>
{
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092"
    };

    using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    
    var csvLine = $"{customer.Gender},{customer.Age},{customer.AnnualIncome},{customer.SpendingScore},{customer.Profession},{customer.WorkExperience},{customer.FamilySize}";

    try
    {
        producer.Produce("csv-data", new Message<Null, string> { Value = csvLine });
        producer.Flush(TimeSpan.FromSeconds(10));
        return Results.Ok(new { message = "Customer queued for processing." });
    }
    catch (ProduceException<Null, string> e)
    {
        Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        return Results.Problem($"Failed to produce message to Kafka: {e.Error.Reason}");
    }
});


app.Run();