namespace ApiYoutubeStats.DTOs
{
    public class RecommendationDto
    {
        public required string VideoId { get; set; }
        public required string Title { get; set; }
        public required string ThumbnailUrl { get; set; }
        public TimeSpan Duration { get; set; }
        public int PlayCount { get; set; }

    }

}
