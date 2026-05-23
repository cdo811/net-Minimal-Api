using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "hello world");

app.MapPost("/upload", async (IFormFile file, IConfiguration config) =>
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded.");

        var connectionString = config.GetConnectionString("DefaultConnection");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
    
        // Read and skip the header row
        var header = await reader.ReadLineAsync();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Create the table if it doesn't exist yet
        var createTableCmd = new SqlCommand(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
        CREATE TABLE Users (Name NVARCHAR(100), Age INT)", connection);
        await createTableCmd.ExecuteNonQueryAsync();

        // Loop through the CSV lines
        // Note: For very large CSVs in production, use SqlBulkCopy instead of looping INSERTS.
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(',');

            using var command = new SqlCommand("INSERT INTO Users (Name, Age) VALUES (@Name, @Age)", connection);
            command.Parameters.AddWithValue("@Name", values[0].Trim());
            command.Parameters.AddWithValue("@Age", int.Parse(values[1].Trim()));
        
            await command.ExecuteNonQueryAsync();
        }

        return Results.Ok("CSV uploaded and data saved to SQL Server.");
    })
    .DisableAntiforgery(); // Disables CSRF tokens so you can test easily via Postman or cURL


app.Run();