namespace ApiYoutubeStats.DTOs
{
    public class HistoryItemDto
    {
        public required string VideoId { get; set; }
        public required string Title { get; set; }
        public DateTime PlayedAt { get; set; }
        public string? ThumbnailUrl { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Tags { get; set; }

    }


}
