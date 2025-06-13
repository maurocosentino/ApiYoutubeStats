using ApiYoutubeStats.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ApiYoutubeStats.Services.Implementations
{
    public class RecommendationEngine
    {
        private readonly AppDbContext _context;

        public RecommendationEngine(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecommendationDto>> GetSmartRecommendationsAsync(int limit = 10)
        {
            var userHistoryIds = await _context.HistoryItems
                .Select(h => h.VideoId)
                .Distinct()
                .ToListAsync();

            var recommendations = await _context.PlaybackStats
                .Where(x => !userHistoryIds.Contains(x.VideoId))
                .GroupBy(x => x.VideoId)
                .Select(g => new RecommendationDto
                {
                    VideoId = g.Key,
                    Title = g.First().Title,
                    ThumbnailUrl = g.First().ThumbnailUrl,
                    Duration = g.First().Duration,
                    PlayCount = g.Count()
                })
                .OrderByDescending(r => r.PlayCount)
                .Take(limit)
                .ToListAsync();

            return recommendations;
        }

        public async Task<List<RecommendationDto>> GetSmartRecommendationsByTagsAsync(int limit = 10)
        {
            var userHistory = await _context.HistoryItems
                .OrderByDescending(h => h.PlayedAt)
                .Take(30)
                .ToListAsync();

            var listenedIds = userHistory
                .Select(h => h.VideoId)
                .Distinct()
                .ToList();

            var tagList = userHistory
                .Where(h => !string.IsNullOrEmpty(h.Tags))
                .SelectMany(h => h.Tags!.Split(','))
                .Select(t => t.Trim().ToLower())
                .Distinct()
                .ToList();

            if (!tagList.Any())
                return await GetSmartRecommendationsAsync(limit);

            var recommendations = await _context.PlaybackStats
                .Where(p => !listenedIds.Contains(p.VideoId) &&
                            !string.IsNullOrEmpty(p.Tags) &&
                            tagList.Any(tag => p.Tags!.ToLower().Contains(tag)))
                .GroupBy(p => p.VideoId)
                .Select(g => new RecommendationDto
                {
                    VideoId = g.Key,
                    Title = g.First().Title,
                    ThumbnailUrl = g.First().ThumbnailUrl,
                    Duration = g.First().Duration,
                    PlayCount = g.Count()
                })
                .OrderByDescending(r => r.PlayCount)
                .Take(limit)
                .ToListAsync();

            return recommendations;
        }
    }
}