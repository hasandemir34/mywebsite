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

    public IActionResult ProjeDetay(int id)
    {
        var proje = _context.Projects.FirstOrDefault(x => x.Id == id);
    if (proje == null)
    {
        return NotFound();
    }
        return View(proje);
    }


    // 1. Sayfayı Görüntüleme (GET)
    [Authorize]
    public IActionResult ProjeEkle() => View();

    // 2. Projeyi Veri Tabanına Kaydetme (POST)
    [HttpPost]
    [Authorize]
    public IActionResult ProjeEkle(Project yeniProje)
    {
    if (ModelState.IsValid)
    {
        _context.Projects.Add(yeniProje);
        _context.SaveChanges();
        return RedirectToAction("Projelerim");
    }
    return View(yeniProje);
    }
    
    // HomeController.cs içine eklenecek
    public IActionResult BlogDetay(int id)
    {
        var blog = _context.Blogs.FirstOrDefault(x => x.Id == id);
        if (blog == null)
        {
            return NotFound();
        }
        return View(blog);
    }
    // --- PROJE GÜNCELLEME VE SİLME ---

[Authorize]
public IActionResult ProjeGuncelle(int id)
{
    var proje = _context.Projects.Find(id);
    if (proje == null) return NotFound();
    return View(proje);
}

[HttpPost] [Authorize]
public IActionResult ProjeGuncelle(Project model)
{
    if (ModelState.IsValid) {
        _context.Update(model);
        _context.SaveChanges();
        return RedirectToAction("Projelerim");
    }
    return View(model);
}

    [Authorize]
    public IActionResult ProjeSil(int id)
    {
        var proje = _context.Projects.Find(id);
        if (proje != null) {
            _context.Projects.Remove(proje);
            _context.SaveChanges();
        }
        return RedirectToAction("Projelerim");
    }

    // --- BLOG GÜNCELLEME VE SİLME ---

    [Authorize]
    public IActionResult BlogGuncelle(int id)
    {
        var blog = _context.Blogs.Find(id);
        if (blog == null) return NotFound();
        return View(blog);
    }

    [HttpPost] [Authorize]
    public IActionResult BlogGuncelle(Blog model)
    {
        if (ModelState.IsValid) {
            _context.Update(model);
            _context.SaveChanges();
            return RedirectToAction("Gunluk");
        }
        return View(model);
    }

    [Authorize]
    public IActionResult BlogSil(int id)
    {
        var blog = _context.Blogs.Find(id);
        if (blog != null) {
            _context.Blogs.Remove(blog);
            _context.SaveChanges();
        }
        return RedirectToAction("Gunluk");
    }
}