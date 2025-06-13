using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class PlaylistItem
{
    public int Id { get; set; }

    [Required]
    public int PlaylistId { get; set; }

    [Required(ErrorMessage = "VideoId es obligatorio.")]
    [StringLength(100, ErrorMessage = "El VideoId no puede superar los 100 caracteres.")]
    public string VideoId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "La duración es obligatoria.")]
    public TimeSpan Duration { get; set; }

    [Url(ErrorMessage = "La URL de la miniatura no es válida.")]
    public string ThumbnailUrl { get; set; } = string.Empty;

    [JsonIgnore]
    public  Playlist? Playlist { get; set; }
}
