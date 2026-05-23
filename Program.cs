using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using New_folder;

var builder = WebApplication.CreateBuilder(args);

// Add the Kafka consumer as a hosted service
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.MapGet("/", () => "Hello World! This is the API for processing CSV files with Kafka.");

// This endpoint now produces messages to a Kafka topic
app.MapPost("/upload", async (IFormFile file, IConfiguration config) =>
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded.");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"]
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
                await producer.ProduceAsync("csv-data", new Message<Null, string> { Value = line });
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


app.Run();
