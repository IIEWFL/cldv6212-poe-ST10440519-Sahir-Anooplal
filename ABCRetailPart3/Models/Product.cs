using Azure;
using Azure.Data.Tables;

namespace ABCRetailPart3.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Products";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}