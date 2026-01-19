using Microsoft.EntityFrameworkCore;
using mywebsite.Models;

namespace mywebsite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs { get; set; }
}