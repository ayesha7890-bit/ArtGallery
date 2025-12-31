using ArtGallery.Data; // Apna DbContext namespace yahan
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models; 
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
// Program.cs mein builder.Services ke section mein ye add karein:

{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("con"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession(); 
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=User}/{id?}" // Home/Index se badal kar User/User kar dein
);
// --- ADMIN SEEDER START ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Check karein ke Admin pehle se maujood to nahi
    if (!context.Users.Any(u => u.Role == "Admin"))
    {
        var adminUser = new ArtGallery.Models.User
        {
            FullName = "shah",
            Email = "shah12@gmail.com",
            // Password ko hash karna zaroori hai login verification ke liye
            Password = BCrypt.Net.BCrypt.HashPassword("admin124"),
            Role = "Admin"
        };

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}
// --- ADMIN SEEDER END ---

app.Run();

app.Run();
