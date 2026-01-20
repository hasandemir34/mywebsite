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

    // --- GENEL SAYFALAR ---

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

    public IActionResult Hakkimda() => View();

    // --- BLOG İŞLEMLERİ (CRUD) ---

    public IActionResult Gunluk()
    {
        var yazilar = _context.Blogs.OrderByDescending(x => x.CreatedDate).ToList();
        return View(yazilar);
    }

    public IActionResult BlogDetay(int id)
    {
        var blog = _context.Blogs.FirstOrDefault(x => x.Id == id);
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
        // --- 1. RESİM YÜKLEME İŞLEMİ ---
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

        // --- 2. TARİH VE SLUG OLUŞTURMA ---
        yeniBlog.CreatedDate = DateTime.Now;
        
        // Yeni eklediğimiz kısım: Başlıktan URL (Slug) üretir
        yeniBlog.Slug = GenerateSlug(yeniBlog.Title);

        // --- 3. KAYIT ---
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
        // --- 1. RESİM GÜNCELLEME İŞLEMİ ---
        if (ImageFile != null && ImageFile.Length > 0)
        {
            // A. ESKİ RESMİ SİL (Sunucuda yer kaplamasın)
            if (!string.IsNullOrEmpty(guncelBlog.ImageUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", guncelBlog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            // B. YENİ RESMİ KAYDET
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
            var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
            using (var stream = new FileStream(newPath, FileMode.Create))
            {
                ImageFile.CopyTo(stream);
            }
            guncelBlog.ImageUrl = "/img/" + fileName;
        }

        // --- 2. SLUG (URL) GÜNCELLEME ---
        // Başlık değişmiş olabilir, URL'i de güncelliyoruz
        guncelBlog.Slug = GenerateSlug(guncelBlog.Title);

        // --- 3. KAYIT ---
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
            // 1. Önce sunucudaki resmi sil
            if (!string.IsNullOrEmpty(silinecekBlog.ImageUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", silinecekBlog.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // 2. Sonra veritabanından sil
            _context.Blogs.Remove(silinecekBlog);
            _context.SaveChanges();
        }
        return RedirectToAction("Gunluk");
    }

    // --- PROJE İŞLEMLERİ ---

    public IActionResult Projelerim()
    {
        var projeler = _context.Projects.ToList();
        return View(projeler);
    }

    public IActionResult ProjeDetay(int id)
    {
        var proje = _context.Projects.FirstOrDefault(x => x.Id == id);
        if (proje == null) return NotFound();
        return View(proje);
    }

    [Authorize]
    public IActionResult ProjeEkle() => View();

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
        if (proje != null) 
        {
            _context.Projects.Remove(proje);
            _context.SaveChanges();
        }
        return RedirectToAction("Projelerim");
    }

    // Bu fonksiyonu HomeController class'ının en altına yapıştır
private string GenerateSlug(string title)
{
    if (string.IsNullOrEmpty(title)) return "";

    // 1. Türkçe karakterleri dönüştür
    string slug = title.ToLowerInvariant();
    slug = slug.Replace("ı", "i").Replace("ö", "o").Replace("ü", "u")
               .Replace("ş", "s").Replace("ğ", "g").Replace("ç", "c");

    // 2. Geçersiz karakterleri sil, boşlukları tire yap
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim();

    // 3. Çoklu tireleri temizle
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

    return slug;
}





}