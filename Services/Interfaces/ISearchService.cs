using ApiYoutubeStats.DTOs;

public interface ISearchService
{
    Task<List<YouTubeSearchResultDto>> SearchYoutubeAsync(string query, int maxResults = 50);
    Task<YouTubePlaylistDto> GetPlaylistInfoAsync(string playlistId);

}
