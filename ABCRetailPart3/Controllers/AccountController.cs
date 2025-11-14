using ABCRetailPart3.Models;
using ABCRetailPart3.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailPart3.Controllers
{
    public class AccountController : Controller
    {
        private readonly IStorageService _storageService; // [4]

        public AccountController(IStorageService storageService)
        {
            _storageService = storageService; // [4]
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(); // [1]
        }

        [HttpPost]
        public async Task<IActionResult> Register(ApplicationUser model, string password)
        {
            if (ModelState.IsValid) // [1]
            {
                try
                {
                    model.UserName = model.Email; // [3]
                    model.Role = "Customer"; // [3]

                    var result = await _storageService.CreateUserAsync(model, password); // [4]
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Registration successful! Please login.";
                        return RedirectToAction("Login"); // [1]
                    }
                    else
                    {
                        ModelState.AddModelError("", "User with this email already exists."); // [1]
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Error creating account. Please try again."); // [1]
                }
            }

            return View(model); // [1]
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(); // [1]
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Please enter both email and password."); // [1]
                return View(); // [1]
            }

            try
            {
                var user = await _storageService.AuthenticateUserAsync(email, password); // [4]
                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString()); // [2]
                    HttpContext.Session.SetString("UserEmail", user.Email); // [2]
                    HttpContext.Session.SetString("UserRole", user.Role); // [2]
                    HttpContext.Session.SetString("UserFirstName", user.FirstName); // [2]

                    TempData["SuccessMessage"] = $"Welcome back, {user.FirstName ?? "User"}!";
                    return RedirectToAction("Index", "Home"); // [1]
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt."); // [1]
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error during login. Please try again."); // [1]
            }

            return View(); // [1]
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // [2]
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home"); // [1]
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var userId = HttpContext.Session.GetString("UserId"); // [2]
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login"); // [1]

            var orders = await _storageService.GetUserOrdersAsync(userId); // [4]
            return View(orders); // [1]
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(); // [1]
        }
    }
}

/*
[1] Microsoft Docs. "Controllers and Action Methods in ASP.NET Core MVC." https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
[2] Microsoft Docs. "Working with Session in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state
[3] Microsoft Docs. "Application Models in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/mvc/models
[4] Microsoft Docs. "Dependency Injection in ASP.NET Core." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
*/
