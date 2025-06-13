namespace ApiYoutubeStats.Models
{
    public class PlaybackStat
    {
        public int Id { get; set; }
        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
        public string? Tags { get; set; }

    }
}
