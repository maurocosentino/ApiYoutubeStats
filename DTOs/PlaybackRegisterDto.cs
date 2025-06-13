using System.ComponentModel.DataAnnotations;

namespace ApiYoutubeStats.DTOs
{
    public class PlaybackRegisterDto
    {
        [Required(ErrorMessage = "VideoId is required.")]
        public string VideoId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "ThumbnailUrl is required.")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required.")]
        public TimeSpan Duration { get; set; }
        public string? Tags { get; set; }
    }


}
