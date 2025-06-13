using System.ComponentModel.DataAnnotations;

public class Playlist
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;
    public List<PlaylistTrack> Tracks { get; set; } = new();

    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}
public class PlaylistTrack
{
    public int Id { get; set; }
    public required string VideoId { get; set; }
    public required string Title { get; set; }
    public int PlaylistId { get; set; }
}