namespace ApiYoutubeStats.DTOs
{
    public class UserStatsDto
    {
        public string TotalListeningTime { get; set; } = "00:00:00";
        public int TotalTracksPlayed { get; set; }
        public RecommendationDto? MostPlayed { get; set; }
    }
}
