using ApiYoutubeStats.DTOs;

namespace ApiYoutubeStats.Services.Interfaces
{
    public interface IStatsService
    {
        Task<List<PlaybackRankingDto>> GetMostPlayedAsync(int? limit = null, DateTime? since = null);
        Task<List<PlaybackRankingDto>> GetTopPlayedAsync(int limit = 10);
        Task<TimeSpan> GetTotalListeningTimeAsync();
        Task<UserStatsDto> GetUserStatsAsync(int days);
        Task<List<PlaybackRankingDto>> GetTop10MostPlayedLastMonthAsync();
        Task RegisterPlaybackAsync(PlaybackRegisterDto dto);
        Task<List<RecommendationDto>> GetSmartRecommendationsAsync(int limit = 10);
        Task<List<RecommendationDto>> GetSmartRecommendationsByTagsAsync(int limit = 10);


    }
}
