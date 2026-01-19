using Microsoft.EntityFrameworkCore;
using mywebsite.Models; // Kendi proje adınla kontrol et


namespace mywebsite.Data; // Bu satır eksik

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs { get; set; } // Veritabanında "Blogs" tablosu oluşturur
}