using Microsoft.Azure.Functions.Worker; 
using Microsoft.Extensions.Logging; 
using Azure.Data.Tables; 

public class OrderProcessorFunction
{
    private readonly TableServiceClient _tableServiceClient; // [3]
    private readonly ILogger _logger; // [2]

    public OrderProcessorFunction(ILoggerFactory loggerFactory)
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureStorageConnectionString"); // [3]
        _tableServiceClient = new TableServiceClient(connectionString); // [3]
        _logger = loggerFactory.CreateLogger<OrderProcessorFunction>(); // [2]
    }

    [Function("OrderProcessor")] // [1]
    public async Task Run([QueueTrigger("orders", Connection = "AzureStorageConnectionString")] string queueItem) // [1]
    {
        _logger.LogInformation($"Processing order: {queueItem}"); // [2]

        try
        {
            var tableClient = _tableServiceClient.GetTableClient("OrderLogs"); // [3]
            await tableClient.CreateIfNotExistsAsync(); // [3]

            var logEntry = new TableEntity("OrderLogs", Guid.NewGuid().ToString()) // [3]
            {
                {"Message", queueItem},
                {"ProcessedAt", DateTime.UtcNow},
                {"Status", "Processed"}
            };

            await tableClient.AddEntityAsync(logEntry); // [3]
            _logger.LogInformation($"Order processed: {queueItem}"); // [2]
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing order: {ex.Message}"); // [2]
        }
    }
}

/*
[1] Microsoft Docs. "Azure Functions triggers and bindings in .NET." https://learn.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings
[2] Microsoft Docs. "Logging in Azure Functions using ILogger." https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library#logging
[3] Microsoft Docs. "Azure Tables client library for .NET." https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables
*/
