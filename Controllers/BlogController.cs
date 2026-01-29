using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using mywebsite.Data;
using mywebsite.Models;
using System.IO;

namespace mywebsite.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BlogController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var yazilar = _context.Blogs.OrderByDescending(x => x.CreatedDate).ToList();
            return View(yazilar);
        }

        public IActionResult Detay(int id)
        {
            var blog = _context.Blogs.Find(id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        [Authorize]
        public IActionResult Ekle()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Ekle(Blog yeniBlog, IFormFile? ImageFile)
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
                return RedirectToAction("Index");
            }
            return View(yeniBlog);
        }

        [Authorize]
        public IActionResult Guncelle(int id)
        {
            var blog = _context.Blogs.Find(id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Guncelle(Blog model, IFormFile? ImageFile)
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
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Sil(int id)
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
            return RedirectToAction("Index");
        }
    }
}