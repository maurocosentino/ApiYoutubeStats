namespace ApiYoutubeStats.DTOs
{
    public class YouTubePlaylistDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Url { get; set; } = "";
        public List<YouTubeSearchResultDto> Videos { get; set; } = new();
    }

}
