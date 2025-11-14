using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Http; 
using ABCRetailPart3.Services; 

namespace ABCRetailPart3.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IStorageService _storageService; // [3]

        public OrdersController(IStorageService storageService)
        {
            _storageService = storageService; // [3]
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId"); // [2]
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account"); // [1]

            var orders = await _storageService.GetUserOrdersAsync(userId); // [3]
            return View(orders); // [1]
        }
    }
}

/*
[1] Microsoft Docs. "Controllers and Action Methods in ASP.NET Core MVC." https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
[2] Microsoft Docs. "Working with Session in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state
[3] Microsoft Docs. "Dependency Injection in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
*/
