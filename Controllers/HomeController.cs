using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using mywebsite.Data;
using mywebsite.Models;

namespace mywebsite.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment; // 1. ADIM: Ortam bilgilerine erişim için bu alanı ekledik

    public HomeController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment; // 2. ADIM: Constructor üzerinden enjekte ettik
    }

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
                // 3. ADIM: wwwroot/img klasörünün tam yolunu güvenli bir şekilde alıyoruz
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                
                // 4. ADIM: Klasör yoksa inşa ediyoruz (Hatanın ana çözümü burası)
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
    public IActionResult BlogGuncelle(Blog guncelBlog, IFormFile? ImageFile)
    {
        if (ModelState.IsValid)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // Eski dosyayı silme işlemi
                if (!string.IsNullOrEmpty(guncelBlog.ImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, guncelBlog.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var newPath = Path.Combine(uploadDir, fileName);
                
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
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, silinecekBlog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Blogs.Remove(silinecekBlog);
            _context.SaveChanges();
        }
        return RedirectToAction("Gunluk");
    }

    public IActionResult Projelerim() => View(_context.Projects.ToList());
    public IActionResult ProjeDetay(int id) => View(_context.Projects.Find(id));
}