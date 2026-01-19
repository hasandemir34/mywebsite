using Microsoft.AspNetCore.Mvc;
using mywebsite.Data;
using mywebsite.Models;
using System.Linq;

namespace mywebsite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    // Veritabanı bağlantısını içeri alıyoruz
    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // ANASAYFA: Kendini tanıttığın yer
    public IActionResult Index()
{
    var startDate = DateTime.Now.Date.AddDays(-364);
    var blogCounts = _context.Blogs
        .Where(x => x.CreatedDate >= startDate)
        .GroupBy(x => x.CreatedDate.Date)
        .Select(g => new { Tarih = g.Key, Sayi = g.Count() })
        .ToDictionary(k => k.Tarih, v => v.Sayi);

    ViewBag.BlogCounts = blogCounts;
    return View();
}

    // GÜNLÜK: Blog yazılarının listelendiği yer
    public IActionResult Gunluk()
    {
        // Yazıları tarihe göre tersten (yeniden eskiye) sıralayıp gönderiyoruz
        var yazilar = _context.Blogs.OrderByDescending(x => x.CreatedDate).ToList();
        return View(yazilar);
    }
    
    // Hakkımda sayfası (İstersen Index ile birleştirebiliriz ama ayrı kalsın dersen)
    public IActionResult Hakkimda()
    {
        return View();
    }
}