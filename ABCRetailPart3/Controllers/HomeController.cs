using Microsoft.AspNetCore.Mvc;
using ABCRetailPart3.Services;

namespace ABCRetailPart3.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStorageService _storageService; // [1]

        public HomeController(IStorageService storageService) // [1]
        {
            _storageService = storageService; // [1]
        }

        public IActionResult Index() // [2]
        {
            return View(); // [2]
        }

        public IActionResult Privacy() // [2]
        {
            return View(); // [2]
        }
    }
}

/*
References:
[1] Microsoft, "Dependency injection in ASP.NET Core," Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
[2] Microsoft, "Controllers and actions in ASP.NET Core MVC," Microsoft Learn. https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
*/
