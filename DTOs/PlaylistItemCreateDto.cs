using System.ComponentModel.DataAnnotations;

public class PlaylistItemCreateDto
{
    [Required]
    [StringLength(100)]
    public string VideoId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public TimeSpan Duration { get; set; }

    [Url]
    public string ThumbnailUrl { get; set; } = string.Empty;
}
