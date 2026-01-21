using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using mywebsite.Data;
using mywebsite.Models;
using System.IO;

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

    public IActionResult Index()
    {
        var startDate = DateTime.Now.Date.AddDays(-364);

        // 1. Blog yazılarını tarihlerine göre gruplayıp sayılarını alıyoruz
        var blogCounts = _context.Blogs
            .Where(x => x.CreatedDate >= startDate)
            .GroupBy(x => x.CreatedDate.Date)
            .Select(g => new { Tarih = g.Key, Sayi = g.Count() })
            .ToList();

        // 2. Projeleri tarihlerine göre gruplayıp sayılarını alıyoruz
        var projectCounts = _context.Projects
            .Where(x => x.CreatedDate >= startDate)
            .GroupBy(x => x.CreatedDate.Date)
            .Select(g => new { Tarih = g.Key, Sayi = g.Count() })
            .ToList();

        // 3. İki listeyi birleştirip (Concat) aynı tarihtekileri topluyoruz (Sum)
        var totalCounts = blogCounts
            .Concat(projectCounts)
            .GroupBy(x => x.Tarih)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Sayi));

        ViewBag.BlogCounts = totalCounts;
        return View();
    }

    // --- HAKKIMDA BÖLÜMÜ ---
    public IActionResult Hakkimda() 
    {
        var about = _context.Abouts.FirstOrDefault();
        return View(about);
    }

    [Authorize]
    public IActionResult HakkimdaGuncelle()
    {
        var about = _context.Abouts.FirstOrDefault();
        return View(about);
    }

    [HttpPost]
    [Authorize]
    public IActionResult HakkimdaGuncelle(About model)
    {
        if (ModelState.IsValid)
        {
            var existing = _context.Abouts.FirstOrDefault();
            if (existing == null) {
                _context.Abouts.Add(model);
            } else {
                existing.FullName = model.FullName;
                existing.Title = model.Title;
                existing.University = model.University;
                existing.Description = model.Description;
                existing.Skills = model.Skills;
                _context.Abouts.Update(existing);
            }
            _context.SaveChanges();
            return RedirectToAction("Hakkimda");
        }
        return View(model);
    }

    // --- BLOG BÖLÜMÜ ---
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
    public IActionResult BlogGuncelle(Blog model, IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            var existingBlog = _context.Blogs.Find(model.Id);
            if (existingBlog == null) return NotFound();

            existingBlog.Title = model.Title;
            existingBlog.Content = model.Content;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                if (!string.IsNullOrEmpty(existingBlog.ImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingBlog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var newPath = Path.Combine(uploadDir, fileName);
                
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }
                existingBlog.ImageUrl = "/img/" + fileName;
            }

            _context.SaveChanges();
            return RedirectToAction("Gunluk");
        }
        return View(model);
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

    // --- PROJE BÖLÜMÜ ---
    public IActionResult Projelerim() => View(_context.Projects.ToList());
    public IActionResult ProjeDetay(int id) => View(_context.Projects.Find(id));

    [Authorize]
    public IActionResult ProjeEkle() => View();

    [HttpPost]
    [Authorize]
    public IActionResult ProjeEkle(Project yeniProje)
    {
        if (ModelState.IsValid)
        {
            // KRİTİK: Projenin aktivite haritasında görünmesi için tarihini set ediyoruz
            yeniProje.CreatedDate = DateTime.Now; 
            _context.Projects.Add(yeniProje);
            _context.SaveChanges();
            return RedirectToAction("Projelerim");
        }
        return View(yeniProje);
    }

    [Authorize]
    public IActionResult ProjeGuncelle(int id)
    {
        var proje = _context.Projects.Find(id);
        if (proje == null) return NotFound();
        return View(proje);
    }

    [HttpPost]
    [Authorize]
    public IActionResult ProjeGuncelle(Project model)
    {
        if (ModelState.IsValid)
        {
            // Veritabanındaki orijinal projeyi buluyoruz
            var existingProject = _context.Projects.Find(model.Id);
            if (existingProject == null) return NotFound();

            // Alanları güncelliyoruz, CreatedDate orijinal haliyle korunuyor
            existingProject.Title = model.Title;
            existingProject.Description = model.Description;
            existingProject.GithubLink = model.GithubLink;
            existingProject.Technologies = model.Technologies;
            existingProject.Category = model.Category;
            existingProject.Content = model.Content;

            _context.SaveChanges();
            return RedirectToAction("Projelerim");
        }
        return View(model);
    }

    [Authorize]
    public IActionResult ProjeSil(int id)
    {
        var silinecekProje = _context.Projects.Find(id);
        if (silinecekProje != null)
        {
            _context.Projects.Remove(silinecekProje);
            _context.SaveChanges();
        }
        return RedirectToAction("Projelerim");
    }
}