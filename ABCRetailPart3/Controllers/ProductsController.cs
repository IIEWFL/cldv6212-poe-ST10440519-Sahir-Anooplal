using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ABCRetailPart3.Services;
using ABCRetailPart3.Models;

namespace ABCRetailPart3.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IStorageService _storageService;

        public ProductsController(IStorageService storageService)
        {
            _storageService = storageService; // [1]
        }

        public async Task<IActionResult> Index()
        {
            var products = await _storageService.GetProductsAsync(); // [1]
            return View(products); // [2]
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _storageService.GetProductsAsync(); // [1]

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return View("Index", products); // [2]
        }

        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [2]

            return View(); // [2]
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var imageUrl = await _storageService.UploadImageAsync(imageFile); // [1]
                product.ImageUrl = imageUrl;
            }

            await _storageService.AddProductAsync(product); // [1]
            TempData["Success"] = "Product created successfully!";
            return RedirectToAction("Index"); // [2]
        }
    }
}

/*
[1] Microsoft Docs. "Dependency Injection in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
[2] Microsoft Docs. "Controller Actions and Action Results." https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
*/
