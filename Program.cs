using Microsoft.EntityFrameworkCore;
using mywebsite.Data;
using Microsoft.AspNetCore.Identity;
using mywebsite.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// A. PostgreSQL Tarih Hatasını Çözen Kritik Satır
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 1. Servisleri Ekle
// Dil dosyalarının (Resources) aranacağı klasörü belirtiyoruz
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddRazorPages(); // Identity sayfaları için gerekli

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity (Giriş Sistemi) Konfigürasyonu
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// 3. Dil Seçeneklerini Yapılandır
var supportedCultures = new[]
{
    new CultureInfo("tr-TR"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("tr-TR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// 4. Veritabanı ve Seed İşlemleri (Otomatik Kurulum)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    context.Database.Migrate();

    if (!context.Blogs.Any())
    {
        context.Blogs.Add(new Blog { Title = "İlk Yazım", Content = "Siteme hoş geldiniz!", CreatedDate = DateTime.Now });
        context.SaveChanges();
    }

    if (!context.Projects.Any())
    {
        context.Projects.AddRange(
            new Project 
            { 
                Title = "Kişisel Portfolyo Sitesi", 
                Description = "ASP.NET Core MVC projesi.", 
                Technologies = "C#, ASP.NET Core", 
                Category = "web",
                GithubLink = "https://github.com/hasandemir34/mywebsite",
                CreatedDate = DateTime.Now
            },
            new Project 
            { 
                Title = "Veri Analizi Çalışması", 
                Description = "Python ile yapıldı.", 
                Technologies = "Python, Pandas", 
                Category = "ai",
                GithubLink = "https://github.com/hasandemir34",
                CreatedDate = DateTime.Now
            }
        );
        context.SaveChanges();
    }

    var adminEmail = "admin@hasan.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(user, "Hasan123!");
    }
}

// 5. Middleware Ayarları
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();