using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mywebsite.Models;

namespace mywebsite.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> _logger)
    {
        this._logger = _logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // --- YENİ EKLENEN KISIM BAŞLANGICI ---
    public IActionResult Projelerim() => View();
    public IActionResult Gunluk() => View();
    public IActionResult Hakkimda() => View();
    // --- YENİ EKLENEN KISIM BİTİŞİ ---

   [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
    // Doğru olan 'Activity.Current?.Id' kullanımıdır
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}