using Confluent.Kafka;
using Microsoft.Data.SqlClient;

namespace New_folder;

public class KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration config)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Kafka Consumer Service is starting.");

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "csv-processor",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe("csv-data");

        var connectionString = config.GetConnectionString("DefaultConnection");

 


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                var csvLine = consumeResult.Message.Value;
                logger.LogInformation($"Consumed message: {csvLine}");

                var values = csvLine.Split(',');
               
                if (values.Length != 7)
                {
                    logger.LogWarning($"Skipping invalid CSV line: {csvLine}");
                    continue;
                }

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(stoppingToken);

                using var command = new SqlCommand(
                    "INSERT INTO Customers ( Gender, Age, AnnualIncome, SpendingScore, Profession, WorkExperience, FamilySize) " +
                    "VALUES ( @Gender, @Age, @AnnualIncome, @SpendingScore, @Profession, @WorkExperience, @FamilySize)",
                    connection);

                command.Parameters.AddWithValue("@Gender", values[0].Trim());
                command.Parameters.AddWithValue("@Age", int.Parse(values[1].Trim()));
                command.Parameters.AddWithValue("@AnnualIncome", int.Parse(values[2].Trim()));
                command.Parameters.AddWithValue("@SpendingScore", int.Parse(values[3].Trim()));
                command.Parameters.AddWithValue("@Profession", values[4].Trim());
                command.Parameters.AddWithValue("@WorkExperience", int.Parse(values[5].Trim()));
                command.Parameters.AddWithValue("@FamilySize", int.Parse(values[6].Trim()));

                await command.ExecuteNonQueryAsync(stoppingToken);
                logger.LogInformation($"Successfully inserted CustomerID {values[0]} into the database.");
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while consuming a Kafka message or saving to the database.");
                // In a real app, you might move the failed message to a dead-letter queue.
            }
        }

        consumer.Close();
        logger.LogInformation("Kafka Consumer Service is stopping.");
    }
}