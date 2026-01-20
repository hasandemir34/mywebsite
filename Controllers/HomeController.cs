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

        public IActionResult Hakkimda() 
    {
        // Veritabanındaki ilk (ve tek) hakkımda kaydını çekiyoruz
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
public IActionResult ProjeEkle() 
{
    return View();
}

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

        [Authorize]
    public IActionResult ProjeGuncelle(int id)
    {
        // Düzenlenecek projeyi veritabanından buluyoruz
        var proje = _context.Projects.Find(id);
        if (proje == null) return NotFound();
        return View(proje);
    }

    [HttpPost]
    [Authorize]
    public IActionResult ProjeGuncelle(Project guncelProje)
    {
        if (ModelState.IsValid)
        {
            // View tarafında <input type="hidden" asp-for="Id" /> kullandığın için 
            // ID otomatik olarak buraya gelecek.
            _context.Projects.Update(guncelProje);
            _context.SaveChanges();
            return RedirectToAction("Projelerim");
        }
        return View(guncelProje);
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



    public IActionResult Projelerim() => View(_context.Projects.ToList());
    public IActionResult ProjeDetay(int id) => View(_context.Projects.Find(id));
}