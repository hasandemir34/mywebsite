using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için şart
using Microsoft.AspNetCore.Http;    // IFormFile için şart
using System.IO;                    // Path ve Directory için şart
using mywebsite.Data;
using mywebsite.Models;

namespace mywebsite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
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

    public IActionResult BlogDetay(int id)
    {
        var blog = _context.Blogs.Find(id);
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
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadDir, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
                yeniBlog.ImageUrl = "/img/" + fileName;
            }

            yeniBlog.CreatedDate = DateTime.Now; // Yeni yazıya tarih veriyoruz
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
            // KRİTİK DÜZELTME: Mevcut yazıyı veritabanından çekiyoruz ki tarih kaybolmasın!
            var varOlanBlog = _context.Blogs.Find(guncelBlog.Id);
            if (varOlanBlog == null) return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // Eski resmi fiziksel olarak siliyoruz
                if (!string.IsNullOrEmpty(varOlanBlog.ImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, varOlanBlog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var newPath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
                varOlanBlog.ImageUrl = "/img/" + fileName;
            }

            // Sadece değişen alanları güncelliyoruz, CreatedDate dokunulmaz kalıyor
            varOlanBlog.Title = guncelBlog.Title;
            varOlanBlog.Content = guncelBlog.Content;

            _context.Blogs.Update(varOlanBlog);
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
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, silinecekBlog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Blogs.Remove(silinecekBlog);
            _context.SaveChanges();
        }
        return RedirectToAction("Gunluk");
    }

    // --- PROJE İŞLEMLERİ ---
    public IActionResult Projelerim() => View(_context.Projects.ToList());
    public IActionResult ProjeDetay(int id) => View(_context.Projects.Find(id));
}