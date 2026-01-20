namespace mywebsite.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? GithubLink { get; set; }
    public string? Technologies { get; set; }
    public string? Category { get; set; } // Bu satırı ekle (web, mobile veya ai değerlerini alacak)
    public string? Content { get; set; }
}