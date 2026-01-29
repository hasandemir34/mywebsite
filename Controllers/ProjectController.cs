using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using mywebsite.Data;
using mywebsite.Models;

namespace mywebsite.Controllers
{
    public class ProjectController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() // Eski adı: Projelerim
        {
            return View(_context.Projects.ToList());
        }

        public IActionResult Detay(int id) // Eski adı: ProjeDetay
        {
            var project = _context.Projects.Find(id);
            if (project == null) return NotFound();
            return View(project);
        }

        [Authorize]
        public IActionResult Ekle() // Eski adı: ProjeEkle
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Ekle(Project yeniProje)
        {
            if (ModelState.IsValid)
            {
                yeniProje.CreatedDate = DateTime.Now; 
                _context.Projects.Add(yeniProje);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(yeniProje);
        }

        [Authorize]
        public IActionResult Guncelle(int id) // Eski adı: ProjeGuncelle
        {
            var proje = _context.Projects.Find(id);
            if (proje == null) return NotFound();
            return View(proje);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Guncelle(Project model)
        {
            if (ModelState.IsValid)
            {
                var existingProject = _context.Projects.Find(model.Id);
                if (existingProject == null) return NotFound();

                existingProject.Title = model.Title;
                existingProject.Description = model.Description;
                existingProject.GithubLink = model.GithubLink;
                existingProject.Technologies = model.Technologies;
                existingProject.Category = model.Category;
                existingProject.Content = model.Content;

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Sil(int id) // Eski adı: ProjeSil
        {
            var silinecekProje = _context.Projects.Find(id);
            if (silinecekProje != null)
            {
                _context.Projects.Remove(silinecekProje);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}