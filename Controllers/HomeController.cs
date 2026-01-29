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

    public IActionResult Index()
    {
        var startDate = DateTime.Now.Date.AddDays(-364);

        var blogCounts = _context.Blogs
            .Where(x => x.CreatedDate >= startDate)
            .GroupBy(x => x.CreatedDate.Date)
            .Select(g => new { Tarih = g.Key, Sayi = g.Count() })
            .ToList();

        var projectCounts = _context.Projects
            .Where(x => x.CreatedDate >= startDate)
            .GroupBy(x => x.CreatedDate.Date)
            .Select(g => new { Tarih = g.Key, Sayi = g.Count() })
            .ToList();

        var totalCounts = blogCounts
            .Concat(projectCounts)
            .GroupBy(x => x.Tarih)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Sayi));

        ViewBag.BlogCounts = totalCounts;
        return View();
    }

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
}