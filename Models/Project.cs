namespace mywebsite.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } // Proje Adı
    public string Description { get; set; } // Açıklama
    public string? GithubLink { get; set; } // GitHub Linki (Opsiyonel)
    public string? Technologies { get; set; } // Kullanılan Teknolojiler (C#, Python vb.)
}