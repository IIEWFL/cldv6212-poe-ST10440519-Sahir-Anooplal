using ABCRetailPart3.Data; 
using ABCRetailPart3.Models; 
using ABCRetailPart3.Services; 
using Microsoft.AspNetCore.Http.Features; 
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // [1]

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // [3]

// Add Storage Service
builder.Services.AddScoped<IStorageService, StorageService>(); // [1]

// Add session support
builder.Services.AddDistributedMemoryCache(); // [1]
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // [1]
    options.Cookie.HttpOnly = true; // [1]
    options.Cookie.IsEssential = true; // [1]
});

// For file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB [2]
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // [1]
    app.UseHsts(); // [1]
}

app.UseHttpsRedirection(); // [1]
app.UseStaticFiles(); // [1]
app.UseRouting(); // [1]
app.UseAuthorization(); // [1]
app.UseSession(); // [1]

// Initialize database and create default admin user
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // [1]
    var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>(); // [1]

    // Ensure database is created
    await dbContext.Database.EnsureCreatedAsync(); // [3]

    // Check if admin user exists
    var adminUser = await storageService.AuthenticateUserAsync("admin@abcretail.com", "admin123"); // [1]
    if (adminUser == null)
    {
        var newAdmin = new ApplicationUser // [1]
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@abcretail.com",
            UserName = "admin@abcretail.com",
            Role = "Admin",
            PhoneNumber = "1234567890"
        };
        await storageService.CreateUserAsync(newAdmin, "admin123"); // [1]
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // [1]

app.Run(); 

/*
[1] Microsoft Docs. "ASP.NET Core Application Startup." https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup
[2] Microsoft Docs. "FormOptions Class." https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.features.formoptions
[3] Microsoft Docs. "Entity Framework Core - Getting Started." https://learn.microsoft.com/en-us/ef/core/
*/
