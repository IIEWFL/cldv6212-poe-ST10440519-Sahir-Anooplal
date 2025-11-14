using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ABCRetailPart3.Models;
using ABCRetailPart3.Services;

namespace ABCRetailPart3.Controllers
{
    public class AdminController : Controller
    {
        private readonly IStorageService _storageService; // [4]

        public AdminController(IStorageService storageService)
        {
            _storageService = storageService; // [4]
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole"); // [2]
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [1]

            return View(); // [1]
        }

        public async Task<IActionResult> Orders()
        {
            var userRole = HttpContext.Session.GetString("UserRole"); // [2]
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [1]

            var orders = await _storageService.GetAllOrdersAsync(); // [4]
            return View(orders); // [1]
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var userRole = HttpContext.Session.GetString("UserRole"); // [2]
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [1]

            await _storageService.UpdateOrderStatusAsync(orderId, status); // [4]
            TempData["Success"] = "Order status updated!"; // [1]
            return RedirectToAction("Orders"); // [1]
        }

        public async Task<IActionResult> Users()
        {
            var userRole = HttpContext.Session.GetString("UserRole"); // [2]
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [1]

            var users = await _storageService.GetAllUsersAsync(); // [4]
            return View(users); // [1]
        }

        public async Task<IActionResult> Products()
        {
            var userRole = HttpContext.Session.GetString("UserRole"); // [2]
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [1]

            var products = await _storageService.GetProductsAsync(); // [4]
            return View(products); // [1]
        }
    }
}

/*
[1] Microsoft Docs. "Controllers and Action Methods in ASP.NET Core MVC." https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
[2] Microsoft Docs. "Working with Session in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state
[4] Microsoft Docs. "Dependency Injection in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
*/
