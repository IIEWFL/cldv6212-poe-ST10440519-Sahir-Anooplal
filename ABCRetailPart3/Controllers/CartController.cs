using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Http; 
using ABCRetailPart3.Models; 
using ABCRetailPart3.Services; 
using System.Text.Json; 

namespace ABCRetailPart3.Controllers
{
    public class CartController : Controller
    {
        private readonly IStorageService _storageService; // [4]

        public CartController(IStorageService storageService)
        {
            _storageService = storageService; // [4]
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId"); // [2]
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account"); // [1]

            var cartItems = await _storageService.GetCartItemsAsync(userId); // [4]
            return View(cartItems); // [1]
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            var userId = HttpContext.Session.GetString("UserId"); // [2]
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account"); // [1]

            var product = await _storageService.GetProductAsync(productId); // [4]
            if (product == null)
            {
                TempData["Error"] = "Product not found!";
                return RedirectToAction("Index", "Products"); // [1]
            }

            var cartItem = new CartItem // [3]
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity
            };

            await _storageService.AddToCartAsync(userId, cartItem); // [4]
            TempData["Success"] = "Product added to cart!";
            return RedirectToAction("Index", "Products"); // [1]
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = HttpContext.Session.GetString("UserId"); // [2]
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account"); // [1]

            await _storageService.RemoveCartItemAsync(userId, cartItemId); // [4]
            return RedirectToAction("Index"); // [1]
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(string shippingAddress)
        {
            var userId = HttpContext.Session.GetString("UserId"); // [2]
            var userEmail = HttpContext.Session.GetString("UserEmail"); // [2]
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account"); // [1]

            var cartItems = await _storageService.GetCartItemsAsync(userId); // [4]
            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index"); // [1]
            }

            var totalAmount = cartItems.Sum(item => item.Price * item.Quantity);
            var orderItemsJson = JsonSerializer.Serialize(cartItems); // [5]

            var order = new Order // [3]
            {
                CustomerId = int.Parse(userId),
                CustomerEmail = userEmail!,
                TotalAmount = totalAmount,
                Status = "PENDING",
                ShippingAddress = shippingAddress,
                OrderItemsJson = orderItemsJson
            };

            await _storageService.CreateOrderAsync(order); // [4]
            await _storageService.ClearCartAsync(userId); // [4]

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("Index", "Orders"); // [1]
        }
    }
}

/*
[1] Microsoft Docs. "Controllers and Action Methods in ASP.NET Core MVC." https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
[2] Microsoft Docs. "Working with Session in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state
[3] Microsoft Docs. "Application Models in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/mvc/models
[4] Microsoft Docs. "Dependency Injection in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
[5] Microsoft Docs. "System.Text.Json Overview." https://learn.microsoft.com/en-us/dotnet/api/system.text.json
*/
