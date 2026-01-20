using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using mywebsite.Data;
using mywebsite.Models;

namespace mywebsite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // ANASAYFA: Grafik verilerini çeker
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

    public IActionResult Gunluk()
    {
        var yazilar = _context.Blogs.OrderByDescending(x => x.CreatedDate).ToList();
        return View(yazilar);
    }

    // SADECE GİRİŞ YAPANLAR (ADMİN) YAZI EKLEYEBİLİR
    [Authorize]
    public IActionResult BlogEkle() => View();

    [HttpPost]
    [Authorize]
    public IActionResult BlogEkle(Blog yeniBlog)
    {
        if (ModelState.IsValid)
        {
            yeniBlog.CreatedDate = DateTime.Now;
            _context.Blogs.Add(yeniBlog);
            _context.SaveChanges();
            return RedirectToAction("Gunluk");
        }
        return View(yeniBlog);
    }

    public IActionResult Hakkimda() => View();


    public IActionResult Projelerim()
    {
        var projeler = _context.Projects.ToList();
        return View(projeler);
        
    }
}