using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Bu satır önemli
using Microsoft.EntityFrameworkCore;
using mywebsite.Models;
using Microsoft.AspNetCore.Identity;


namespace mywebsite.Data;

// Sınıfın miras aldığı yeri DbContext yerine IdentityDbContext yapıyoruz
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs { get; set; }
}