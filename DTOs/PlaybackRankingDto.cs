namespace ApiYoutubeStats.DTOs
{
    public class PlaybackRankingDto
    {
        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public int PlayCount { get; set; }
    }
}
