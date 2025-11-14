using ABCRetailPart3.Data; 
using ABCRetailPart3.Models; 
using Azure; 
using Azure.Data.Tables; 
using Azure.Storage.Blobs; 
using Azure.Storage.Files.Shares; 
using Azure.Storage.Queues; 
using Microsoft.EntityFrameworkCore; 
using System.Text; 

namespace ABCRetailPart3.Services
{
    public class StorageService : IStorageService
    {
        private readonly TableServiceClient _tableServiceClient; 
        private readonly BlobServiceClient _blobServiceClient; 
        private readonly QueueServiceClient _queueServiceClient; 
        private readonly ShareServiceClient _fileServiceClient; 
        private readonly ApplicationDbContext _dbContext; 
        private readonly IConfiguration _configuration; 

        public StorageService(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _configuration = configuration; // [1]
            _dbContext = dbContext; // [1]

            var storageConnectionString = _configuration.GetValue<string>("AzureStorageConnectionString"); // [1]
            _tableServiceClient = new TableServiceClient(storageConnectionString); 
            _blobServiceClient = new BlobServiceClient(storageConnectionString); 
            _queueServiceClient = new QueueServiceClient(storageConnectionString); 
            _fileServiceClient = new ShareServiceClient(storageConnectionString); 

            InitializeStorageAsync().Wait(); // [1]
        }

        private async Task InitializeStorageAsync()
        {
            // Create Azure Storage resources
            var tables = new[] { "Products", "Customers" };
            foreach (var tableName in tables)
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName); // [2]
                await tableClient.CreateIfNotExistsAsync(); // [2]
            }

            var blobContainers = new[] { "product-images" };
            foreach (var containerName in blobContainers)
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName); // [2]
                await containerClient.CreateIfNotExistsAsync(); // [2]
            }

            var shareClient = _fileServiceClient.GetShareClient("contracts"); // [2]
            await shareClient.CreateIfNotExistsAsync(); // [2]

            var queues = new[] { "orders", "inventory", "customers", "images" };
            foreach (var queueName in queues)
            {
                var queueClient = _queueServiceClient.GetQueueClient(queueName); // [2]
                await queueClient.CreateIfNotExistsAsync(); // [2]
            }
        }

        // USER MANAGEMENT (SQL SERVER)
        public async Task<bool> CreateUserAsync(ApplicationUser user, string password) // [1]
        {
            try
            {
                var existingUser = await _dbContext.Users // [1]
                    .FirstOrDefaultAsync(u => u.Email == user.Email); // [3]

                if (existingUser != null)
                    return false;

                user.Password = password; // [1]
                _dbContext.Users.Add(user); // [1]
                await _dbContext.SaveChangesAsync(); // [3]
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}"); // [4]
                return false;
            }
        }

        public async Task<ApplicationUser?> AuthenticateUserAsync(string email, string password) 
        {
            return await _dbContext.Users // [1]
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password); // [3]
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId) // [1]
        {
            if (int.TryParse(userId, out int id)) // [4]
            {
                return await _dbContext.Users.FindAsync(id); // [3]
            }
            return null;
        }

        // CART MANAGEMENT (SQL SERVER)
        public async Task AddToCartAsync(string userId, CartItem cartItem) // [1]
        {
            if (int.TryParse(userId, out int id)) // [4]
            {
                cartItem.UserId = id; 
                _dbContext.CartItems.Add(cartItem); 
                await _dbContext.SaveChangesAsync(); // [3]
            }
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId) 
        {
            if (int.TryParse(userId, out int id)) // [4]
            {
                return await _dbContext.CartItems // [1]
                    .Where(ci => ci.UserId == id) // [3]
                    .ToListAsync(); 
            }
            return new List<CartItem>(); 
        }

        public async Task RemoveCartItemAsync(string userId, int cartItemId) 
        {
            if (int.TryParse(userId, out int id)) // [4]
            {
                var cartItem = await _dbContext.CartItems // [1]
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.UserId == id); // [3]

                if (cartItem != null)
                {
                    _dbContext.CartItems.Remove(cartItem); 
                    await _dbContext.SaveChangesAsync(); 
                }
            }
        }

        public async Task ClearCartAsync(string userId) 
        {
            if (int.TryParse(userId, out int id)) // [4]
            {
                var cartItems = await _dbContext.CartItems 
                    .Where(ci => ci.UserId == id) 
                    .ToListAsync(); 
                _dbContext.CartItems.RemoveRange(cartItems); 
                await _dbContext.SaveChangesAsync(); // [3]
            }
        }

        // ORDER MANAGEMENT (SQL SERVER)
        public async Task<Order> CreateOrderAsync(Order order) 
        {
            if (int.TryParse(order.CustomerId.ToString(), out int customerId)) // [4]
            {
                order.CustomerId = customerId; 
                _dbContext.Orders.Add(order); 
                await _dbContext.SaveChangesAsync(); 
                await AddQueueMessageAsync("orders", $"Order placed: {order.Id}"); // [2]
                return order;
            }
            throw new Exception("Invalid customer ID"); // [4]
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId) 
        {
            if (int.TryParse(userId, out int id)) // [4]
            {
                return await _dbContext.Orders 
                    .Where(o => o.CustomerId == id) 
                    .OrderByDescending(o => o.OrderDate) 
                    .ToListAsync(); 
            }
            return new List<Order>(); 
        }

        public async Task<List<Order>> GetAllOrdersAsync() 
        {
            return await _dbContext.Orders 
                .Include(o => o.Customer) 
                .OrderByDescending(o => o.OrderDate) 
                .ToListAsync(); // [3]
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status) 
        {
            var order = await _dbContext.Orders.FindAsync(orderId); // [3]
            if (order != null)
            {
                order.Status = status; 
                await _dbContext.SaveChangesAsync(); // [3]
                await AddQueueMessageAsync("orders", $"Order status updated: {orderId} to {status}"); // [2]
            }
        }

        // PRODUCT MANAGEMENT (AZURE TABLE STORAGE)
        public async Task AddProductAsync(Product product) 
        {
            var tableClient = _tableServiceClient.GetTableClient("Products"); // [2]
            product.RowKey = Guid.NewGuid().ToString(); // [4]
            await tableClient.AddEntityAsync(product); // [2]
            await AddQueueMessageAsync("inventory", $"Product created: {product.Name}"); // [2]
        }

        public async Task<List<Product>> GetProductsAsync() 
        {
            var tableClient = _tableServiceClient.GetTableClient("Products"); // [2]
            var products = new List<Product>(); // [4]
            await foreach (var product in tableClient.QueryAsync<Product>()) // [2]
            {
                products.Add(product); 
            }
            return products; 
        }

        public async Task<Product?> GetProductAsync(string productId) 
        {
            var tableClient = _tableServiceClient.GetTableClient("Products"); // [2]
            try
            {
                var product = await tableClient.GetEntityAsync<Product>("Products", productId); // [2]
                return product.Value; 
            }
            catch
            {
                return null; 
            }
        }

        public async Task UpdateProductAsync(Product product) 
        {
            var tableClient = _tableServiceClient.GetTableClient("Products"); // [2]
            await tableClient.UpdateEntityAsync(product, ETag.All); // [2]
            await AddQueueMessageAsync("inventory", $"Product updated: {product.Name}"); // [2]
        }

        public async Task DeleteProductAsync(string productId)
        {
            var tableClient = _tableServiceClient.GetTableClient("Products"); // [2]
            await tableClient.DeleteEntityAsync("Products", productId); // [2]
            await AddQueueMessageAsync("inventory", $"Product deleted: {productId}"); // [2]
        }

        // AZURE STORAGE SERVICES
        public async Task AddCustomerAsync(string name, string email, string phone) 
        {
            var tableClient = _tableServiceClient.GetTableClient("Customers"); // [2]
            var customer = new TableEntity("Customers", Guid.NewGuid().ToString()) // [2]
            {
                {"Name", name},
                {"Email", email},
                {"Phone", phone},
                {"CreatedDate", DateTime.UtcNow}
            };
            await tableClient.AddEntityAsync(customer); // [2]
            await AddQueueMessageAsync("customers", $"Customer added: {name}"); // [2]
        }

        public async Task<List<TableEntity>> GetCustomersAsync() 
        {
            var tableClient = _tableServiceClient.GetTableClient("Customers"); // [2]
            var customers = new List<TableEntity>(); // [4]
            await foreach (var customer in tableClient.QueryAsync<TableEntity>()) // [2]
            {
                customers.Add(customer); 
            }
            return customers; 
        }

        public async Task<string> UploadImageAsync(IFormFile file) 
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("product-images"); // [2]
            await containerClient.CreateIfNotExistsAsync(); // [2]

            var blobName = $"{Guid.NewGuid()}_{file.FileName}"; // [4]
            var blobClient = containerClient.GetBlobClient(blobName); // [2]

            using var stream = file.OpenReadStream(); // [4]
            await blobClient.UploadAsync(stream, true); // [2]
            await AddQueueMessageAsync("images", $"Image uploaded: {file.FileName}"); // [2]

            return blobClient.Uri.ToString(); // [4]
        }

        public async Task<List<string>> GetBlobUrlsAsync() 
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("product-images"); // [2]
            await containerClient.CreateIfNotExistsAsync(); // [2]

            var urls = new List<string>(); // [4]
            await foreach (var blobItem in containerClient.GetBlobsAsync()) // [2]
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name); // [2]
                urls.Add(blobClient.Uri.ToString()); // [4]
            }
            return urls; 
        }

        public async Task AddQueueMessageAsync(string queueName, string message) 
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName); // [2]
            await queueClient.CreateIfNotExistsAsync(); // [2]
            await queueClient.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(message))); // [4]
        }

        public async Task<List<string>> GetQueueMessagesAsync(string queueName) 
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName); // [2]
            await queueClient.CreateIfNotExistsAsync(); // [2]

            var messages = new List<string>(); // [4]
            var receivedMessages = await queueClient.ReceiveMessagesAsync(10); // [2]
            foreach (var message in receivedMessages.Value)
            {
                messages.Add(Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText))); // [4]
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt); // [2]
            }
            return messages; 
        }

        public async Task UploadContractAsync(IFormFile file) 
        {
            var shareClient = _fileServiceClient.GetShareClient("contracts"); // [2]
            await shareClient.CreateIfNotExistsAsync(); // [2]

            var directoryClient = shareClient.GetRootDirectoryClient(); // [2]
            var fileClient = directoryClient.GetFileClient(file.FileName); // [2]

            using var stream = file.OpenReadStream(); // [4]
            await fileClient.CreateAsync(stream.Length); // [2]
            await fileClient.UploadRangeAsync(new Azure.HttpRange(0, stream.Length), stream); // [2]
        }

        public async Task<List<string>> GetContractsAsync() 
        {
            var shareClient = _fileServiceClient.GetShareClient("contracts"); // [2]
            if (!await shareClient.ExistsAsync()) return new List<string>(); // [2]

            var directoryClient = shareClient.GetRootDirectoryClient(); // [2]
            var files = new List<string>(); // [4]
            await foreach (var fileItem in directoryClient.GetFilesAndDirectoriesAsync()) // [2]
            {
                files.Add(fileItem.Name); // [4]
            }
            return files; 
        }

        public Task<List<ApplicationUser>> GetAllUsersAsync() // [1]
        {
            throw new NotImplementedException(); // [4]
        }
    }
}

/*
[1] Microsoft Docs. "Application Models in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/mvc/models
[2] Microsoft Docs. "Azure Storage client libraries." https://learn.microsoft.com/en-us/azure/storage/
[3] Microsoft Docs. "Entity Framework Core - Getting Started." https://learn.microsoft.com/en-us/ef/core/
[4] Microsoft Docs. "C# Programming Guide." https://learn.microsoft.com/en-us/dotnet/csharp/
*/
