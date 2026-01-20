namespace mywebsite.Models;

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    // Slug alanını silebilir veya devre dışı bırakabilirsin
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}