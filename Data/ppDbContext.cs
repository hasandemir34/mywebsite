using Microsoft.EntityFrameworkCore;
using mywebsite.Models; // Kendi proje adınla kontrol et

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs { get; set; } // Veritabanında "Blogs" tablosu oluşturur
}