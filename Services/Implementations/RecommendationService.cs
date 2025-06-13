using ApiYoutubeStats.DTOs;
using ApiYoutubeStats.Services.Implementations;
using ApiYoutubeStats.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiYoutubeStats.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly RecommendationEngine _engine;

        public RecommendationService(AppDbContext context, RecommendationEngine engine)
        {
            _context = context;
            _engine = engine;
        }

        public Task<List<RecommendationDto>> GetSmartRecommendationsAsync(int limit = 10)
        {
            return _engine.GetSmartRecommendationsAsync(limit);
        }

        public Task<List<RecommendationDto>> GetSmartRecommendationsByTagsAsync(int limit = 10)
        {
            return _engine.GetSmartRecommendationsByTagsAsync(limit);
        }

        public async Task<List<RecommendationDto>> GetRecommendationsAsync(int limit = 10)
        {
            return await _context.Favorites
                .Select(x => new RecommendationDto
                {
                    VideoId = x.VideoId,
                    Title = x.Title,
                    ThumbnailUrl = x.ThumbnailUrl
                })
                .Take(limit)
                .ToListAsync();
        }
    }
}