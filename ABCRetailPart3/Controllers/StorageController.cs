using Microsoft.AspNetCore.Mvc;
using ABCRetailPart3.Services;

namespace ABCRetailPart3.Controllers
{
    public class StorageController : Controller
    {
        private readonly IStorageService _storageService;

        public StorageController(IStorageService storageService)
        {
            _storageService = storageService; // [1]
        }

        // Customer Management Page
        public async Task<IActionResult> Customers()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [2]

            var customers = await _storageService.GetCustomersAsync(); // [1]
            return View(customers); // [2]
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer(string name, string email, string phone)
        {
            await _storageService.AddCustomerAsync(name, email, phone); // [1]
            return RedirectToAction("Customers"); // [2]
        }

        // Blob Storage Page
        public async Task<IActionResult> Images()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [2]

            var imageUrls = await _storageService.GetBlobUrlsAsync(); // [1]
            return View(imageUrls); // [2]
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                await _storageService.UploadImageAsync(imageFile); // [1]
            }
            return RedirectToAction("Images"); // [2]
        }

        // Queue Messages Page
        public async Task<IActionResult> Queues()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [2]

            ViewBag.OrderMessages = await _storageService.GetQueueMessagesAsync("orders"); // [1]
            ViewBag.InventoryMessages = await _storageService.GetQueueMessagesAsync("inventory"); // [1]
            ViewBag.CustomerMessages = await _storageService.GetQueueMessagesAsync("customers"); // [1]
            ViewBag.ImageMessages = await _storageService.GetQueueMessagesAsync("images"); // [1]

            return View(); // [2]
        }

        [HttpPost]
        public async Task<IActionResult> AddQueueMessage(string queueName, string message)
        {
            await _storageService.AddQueueMessageAsync(queueName, message); // [1]
            return RedirectToAction("Queues"); // [2]
        }

        // File Shares Page
        public async Task<IActionResult> Contracts()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
                return RedirectToAction("Login", "Account"); // [2]

            var contracts = await _storageService.GetContractsAsync(); // [1]
            return View(contracts); // [2]
        }

        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile contractFile)
        {
            if (contractFile != null && contractFile.Length > 0)
            {
                await _storageService.UploadContractAsync(contractFile); // [1]
            }
            return RedirectToAction("Contracts"); // [2]
        }
    }
}

/*
[1] Microsoft Docs. "Dependency Injection in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
[2] Microsoft Docs. "Controller Actions and Action Results." https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
*/
