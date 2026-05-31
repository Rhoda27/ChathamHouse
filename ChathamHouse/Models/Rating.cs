using System.ComponentModel.DataAnnotations;

public class Rating
{
    public int Id { get; set; }

    public int? PostId { get; set; }  // ← nullable, remove [Required]

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Stars { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}