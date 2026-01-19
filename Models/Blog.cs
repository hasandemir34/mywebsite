using System.ComponentModel.DataAnnotations;

public class Blog
{
    [Key] // Bu alanın Primary Key (Birincil Anahtar) olduğunu belirtir
    public int Id { get; set; }

    [Required(ErrorMessage = "Başlık boş bırakılamaz.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}