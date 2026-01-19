using Microsoft.EntityFrameworkCore;
using mywebsite.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC ve Veritabanı Servislerini Ekle
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Temel Ayarlar
app.UseStaticFiles(); // CSS/JS dosyaları için
app.UseRouting();     // Sayfa yönlendirmeleri için

// 3. Rota Tanımı
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();