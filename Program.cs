using Microsoft.EntityFrameworkCore;
using mywebsite.Data;
using Microsoft.AspNetCore.Identity;
using mywebsite.Models;

var builder = WebApplication.CreateBuilder(args);

// A. PostgreSQL Tarih Hatasını Çözen Kritik Satır
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// 1. Servisleri Ekle
builder.Services.AddControllersWithViews();
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

// 3. Veritabanı ve Admin Seed İşlemi (Otomatik Kurulum)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Veritabanını otomatik oluştur/güncelle
    context.Database.Migrate();

    // Örnek Blog Yazıları
    if (!context.Blogs.Any())
    {
        context.Blogs.Add(new Blog { Title = "İlk Yazım", Content = "Siteme hoş geldiniz!", CreatedDate = DateTime.Now });
        context.SaveChanges();
    }

    // İlk Admin Kullanıcısını Oluştur (Giriş yapabilmen için)
    var adminEmail = "admin@hasan.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(user, "Hasan123!"); // Şifren bu olacak
    }
}

// 4. Middleware Ayarları
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Kimsin?
app.UseAuthorization();  // Yetkin ne?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity sayfalarını aktif et

app.Run();