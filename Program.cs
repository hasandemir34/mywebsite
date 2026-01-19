using Microsoft.EntityFrameworkCore;
using mywebsite.Data;
using Microsoft.AspNetCore.Identity;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// 1. MVC ve Veritabanı Servislerini Ekle
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



// Identity Servislerini Ekle
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();




var app = builder.Build();

// 2. Temel Ayarlar
app.UseStaticFiles(); // CSS/JS dosyaları için

app.UseRouting();
app.UseAuthentication(); // Kimlik doğrulama
app.UseAuthorization();  // Yetkilendirme



app.UseRouting();     // Sayfa yönlendirmeleri için

// 3. Rota Tanımı
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();