using Microsoft.Azure.Functions.Worker; 
using Microsoft.Extensions.Logging; 
using Azure.Data.Tables; 

public class ImageProcessorFunction
{
    private readonly TableServiceClient _tableServiceClient; // [3]
    private readonly ILogger _logger; // [2]

    public ImageProcessorFunction(ILoggerFactory loggerFactory)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString"); // [3]
        _tableServiceClient = new TableServiceClient(connectionString); // [3]
        _logger = loggerFactory.CreateLogger<ImageProcessorFunction>(); // [2]
    }

    [Function("ImageProcessor")] // [1]
    public async Task Run([QueueTrigger("images", Connection = "AzureStorageConnectionString")] string queueItem) // [1]
    {
        _logger.LogInformation($"Processing image: {queueItem}"); // [2]

        try
        {
            var tableClient = _tableServiceClient.GetTableClient("ImageLogs"); // [3]
            await tableClient.CreateIfNotExistsAsync(); // [3]

            var logEntry = new TableEntity("ImageLogs", Guid.NewGuid().ToString()) // [3]
            {
                {"Message", queueItem},
                {"ProcessedAt", DateTime.UtcNow},
                {"Status", "Processed"}
            };

            await tableClient.AddEntityAsync(logEntry); // [3]
            _logger.LogInformation($"Image processed: {queueItem}"); // [2]
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing image: {ex.Message}"); // [2]
        }
    }
}

/*
[1] Microsoft Docs. "Azure Functions triggers and bindings in .NET." https://learn.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings
[2] Microsoft Docs. "Logging in Azure Functions using ILogger." https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library#logging
[3] Microsoft Docs. "Azure Tables client library for .NET." https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables
*/
