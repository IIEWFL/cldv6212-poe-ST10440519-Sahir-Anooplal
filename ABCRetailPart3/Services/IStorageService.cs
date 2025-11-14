using ABCRetailPart3.Models;
using Azure.Data.Tables;

namespace ABCRetailPart3.Services
{
    public interface IStorageService
    {
        // User Management (SQL Server)
        Task<bool> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser?> AuthenticateUserAsync(string email, string password);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<List<ApplicationUser>> GetAllUsersAsync();

        // Cart Management (SQL Server)
        Task AddToCartAsync(string userId, CartItem cartItem);
        Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task RemoveCartItemAsync(string userId, int cartItemId);
        Task ClearCartAsync(string userId);

        // Order Management (SQL Server)
        Task<Order> CreateOrderAsync(Order order);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task UpdateOrderStatusAsync(int orderId, string status);

        // Product Management (Azure Table Storage)
        Task AddProductAsync(Product product);
        Task<List<Product>> GetProductsAsync();
        Task<Product?> GetProductAsync(string productId);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(string productId);

        // Azure Storage Services
        Task AddCustomerAsync(string name, string email, string phone);
        Task<List<TableEntity>> GetCustomersAsync();
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> GetBlobUrlsAsync();
        Task AddQueueMessageAsync(string queueName, string message);
        Task<List<string>> GetQueueMessagesAsync(string queueName);
        Task UploadContractAsync(IFormFile file);
        Task<List<string>> GetContractsAsync();
    }
}