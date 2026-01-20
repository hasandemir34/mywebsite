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

    // ANASAYFA
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

    public IActionResult Hakkimda() => View();

    // --- BLOG İŞLEMLERİ ---

    public IActionResult Gunluk()
    {
        var yazilar = _context.Blogs.OrderByDescending(x => x.CreatedDate).ToList();
        return View(yazilar);
    }

    // SADECE ID İLE ÇALIŞAN DETAY SAYFASI
    
    public IActionResult BlogDetay(string slug)
    {
        var blog = _context.Blogs.FirstOrDefault(x => x.Slug == slug);
        if (blog == null) return NotFound();
        return View(blog);
    }

    [Authorize]
    public IActionResult BlogEkle() => View();

    [HttpPost]
    [Authorize]
    public IActionResult BlogEkle(Blog yeniBlog, IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
                yeniBlog.ImageUrl = "/img/" + fileName;
            }

            yeniBlog.CreatedDate = DateTime.Now;
            
            _context.Blogs.Add(yeniBlog);
            _context.SaveChanges();
            return RedirectToAction("Gunluk");
        }
        return View(yeniBlog);
    }

    [Authorize]
    public IActionResult BlogGuncelle(int id)
    {
        var blog = _context.Blogs.Find(id);
        if (blog == null) return NotFound();
        return View(blog);
    }

    [HttpPost]
    [Authorize]
    public IActionResult BlogGuncelle(Blog guncelBlog, IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(guncelBlog.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", guncelBlog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
                guncelBlog.ImageUrl = "/img/" + fileName;
            }

            _context.Blogs.Update(guncelBlog);
            _context.SaveChanges();
            return RedirectToAction("Gunluk");
        }
        return View(guncelBlog);
    }

    [Authorize]
    public IActionResult BlogSil(int id)
    {
        var silinecekBlog = _context.Blogs.Find(id);
        if (silinecekBlog != null)
        {
            if (!string.IsNullOrEmpty(silinecekBlog.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", silinecekBlog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Blogs.Remove(silinecekBlog);
            _context.SaveChanges();
        }
        return RedirectToAction("Gunluk");
    }

    // --- PROJE İŞLEMLERİ (Aynen Kalıyor) ---
    public IActionResult Projelerim() => View(_context.Projects.ToList());
    public IActionResult ProjeDetay(int id) => View(_context.Projects.Find(id));
    
    // ... Diğer Proje metodları ...
}