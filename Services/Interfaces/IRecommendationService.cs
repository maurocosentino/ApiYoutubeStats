using ApiYoutubeStats.DTOs;

namespace ApiYoutubeStats.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<RecommendationDto>> GetRecommendationsAsync(int limit = 10);
        Task<List<RecommendationDto>> GetSmartRecommendationsAsync(int limit = 10);
        Task<List<RecommendationDto>> GetSmartRecommendationsByTagsAsync(int limit = 10);

    }
}
