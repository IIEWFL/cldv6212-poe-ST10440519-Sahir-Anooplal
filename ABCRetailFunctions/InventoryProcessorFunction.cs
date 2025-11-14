using Microsoft.Azure.Functions.Worker; 
using Microsoft.Extensions.Logging; 
using Azure.Data.Tables; 

public class InventoryProcessorFunction
{
    private readonly TableServiceClient _tableServiceClient; // [3]
    private readonly ILogger _logger; // [2]

    public InventoryProcessorFunction(ILoggerFactory loggerFactory)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString"); // [3]
        _tableServiceClient = new TableServiceClient(connectionString); // [3]
        _logger = loggerFactory.CreateLogger<InventoryProcessorFunction>(); // [2]
    }

    [Function("InventoryProcessor")] // [1]
    public async Task Run([QueueTrigger("inventory", Connection = "AzureStorageConnectionString")] string queueItem) // [1]
    {
        _logger.LogInformation($"Processing inventory: {queueItem}"); // [2]

        try
        {
            var tableClient = _tableServiceClient.GetTableClient("InventoryLogs"); // [3]
            await tableClient.CreateIfNotExistsAsync(); // [3]

            var logEntry = new TableEntity("InventoryLogs", Guid.NewGuid().ToString()) // [3]
            {
                {"Message", queueItem},
                {"ProcessedAt", DateTime.UtcNow},
                {"Status", "Processed"}
            };

            await tableClient.AddEntityAsync(logEntry); // [3]
            _logger.LogInformation($"Inventory processed: {queueItem}"); // [2]
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing inventory: {ex.Message}"); // [2]
        }
    }
}

/*
[1] Microsoft Docs. "Azure Functions triggers and bindings in .NET." https://learn.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings
[2] Microsoft Docs. "Logging in Azure Functions using ILogger." https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library#logging
[3] Microsoft Docs. "Azure Tables client library for .NET." https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables
*/
