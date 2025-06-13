public interface ISearchService
{
    Task<List<YouTubeSearchResultDto>> SearchYoutubeAsync(string query, int maxResults = 50);
}
