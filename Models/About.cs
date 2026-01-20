namespace mywebsite.Models;

public class About
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Title { get; set; }
    public string University { get; set; }
    public string Description { get; set; }
    public string? Skills { get; set; } // Virgülle ayrılmış yetenekler
}