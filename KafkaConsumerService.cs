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

        // Ensure the table exists before starting the consumer loop.
        try
        {
            await using var initialConnection = new SqlConnection(connectionString);
            await initialConnection.OpenAsync(stoppingToken);
            var createTableCmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
                CREATE TABLE Customers (
                    CustomerID INT PRIMARY KEY,
                    Gender VARCHAR(10),
                    Age INT,
                    AnnualIncome INT,
                    SpendingScore INT,
                    Profession VARCHAR(50),
                    WorkExperience INT,
                    FamilySize INT
                );", initialConnection);
            await createTableCmd.ExecuteNonQueryAsync(stoppingToken);
            logger.LogInformation("Successfully ensured 'Customers' table exists.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring database table exists. The consumer will not start.");
            return; // Stop execution if we can't connect to the DB or create the table.
        }


        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                var csvLine = consumeResult.Message.Value;
                logger.LogInformation($"Consumed message: {csvLine}");

                var values = csvLine.Split(',');
                if (values.Length != 8)
                {
                    logger.LogWarning($"Skipping invalid CSV line: {csvLine}");
                    continue;
                }

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(stoppingToken);

                using var command = new SqlCommand(
                    "INSERT INTO Customers (CustomerID, Gender, Age, AnnualIncome, SpendingScore, Profession, WorkExperience, FamilySize) " +
                    "VALUES (@CustomerID, @Gender, @Age, @AnnualIncome, @SpendingScore, @Profession, @WorkExperience, @FamilySize)",
                    connection);

                command.Parameters.AddWithValue("@CustomerID", int.Parse(values[0].Trim()));
                command.Parameters.AddWithValue("@Gender", values[1].Trim());
                command.Parameters.AddWithValue("@Age", int.Parse(values[2].Trim()));
                command.Parameters.AddWithValue("@AnnualIncome", int.Parse(values[3].Trim()));
                command.Parameters.AddWithValue("@SpendingScore", int.Parse(values[4].Trim()));
                command.Parameters.AddWithValue("@Profession", values[5].Trim());
                command.Parameters.AddWithValue("@WorkExperience", int.Parse(values[6].Trim()));
                command.Parameters.AddWithValue("@FamilySize", int.Parse(values[7].Trim()));

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