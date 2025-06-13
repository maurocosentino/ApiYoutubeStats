using ApiYoutubeStats.DTOs;
using ApiYoutubeStats.Models;
using ApiYoutubeStats.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiYoutubeStats.Services.Implementations
{
    public class StatsService : IStatsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStatsCache _cache; 
        private readonly RecommendationEngine _engine;

        public StatsService(AppDbContext context, IMapper mapper, IStatsCache cache, RecommendationEngine engine)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _engine = engine;
        }

        public async Task<List<PlaybackRankingDto>> GetMostPlayedAsync(int? limit = null, DateTime? since = null)
        {
            string cacheKey = $"stats:mostPlayed:{limit}:{since?.ToString("yyyyMMdd")}";

            if (_cache.TryGet(cacheKey, out List<PlaybackRankingDto>? cached) && cached is not null)
                return cached;

            var query = _context.PlaybackStats.AsQueryable();

            if (since.HasValue)
                query = query.Where(x => x.PlayedAt >= since.Value);

            var resultQuery = GetPlaybackRankingQuery(query);

            if (limit.HasValue)
                resultQuery = resultQuery.Take(limit.Value);

            var result = await resultQuery.ToListAsync();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
            return result;
        }


        public async Task RegisterPlaybackAsync(PlaybackRegisterDto dto)
        {
            var stat = new PlaybackStat
            {
                VideoId = dto.VideoId,
                Title = dto.Title,
                ThumbnailUrl = dto.ThumbnailUrl,
                Duration = dto.Duration,
                PlayedAt = DateTime.UtcNow,
                Tags = dto.Tags ?? "otros"
            };

            _context.PlaybackStats.Add(stat);
            await _context.SaveChangesAsync();


            
            _cache.Remove("stats:topPlayed:10");                                  
            _cache.Remove("stats:user:1");                                       
            _cache.Remove("stats:user:7");                                       
            _cache.Remove("stats:totalListeningTime");                         
        }

        public Task<List<PlaybackRankingDto>> GetTopPlayedAsync(int limit = 10)
        {
            var cacheKey = $"stats:topPlayed:{limit}";

            if (_cache.TryGet<List<PlaybackRankingDto>>(cacheKey, out var cached) && cached is not null)
                return Task.FromResult(cached);

            return GetMostPlayedAsync(limit);
        }

        public async Task<TimeSpan> GetTotalListeningTimeAsync()
        {
            const string cacheKey = "stats:totalListeningTime";

            if (_cache.TryGet(cacheKey, out TimeSpan cachedTime))                 
                return cachedTime;

            var durations = await _context.PlaybackStats
                .Select(x => x.Duration)
                .ToListAsync();

            var totalSeconds = durations.Sum(d => d.TotalSeconds);
            var total = TimeSpan.FromSeconds(totalSeconds);
            _cache.Set(cacheKey, total, TimeSpan.FromMinutes(10));               
            return total;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(int days)
        {
            var cacheKey = $"stats:user:{days}";

            if (_cache.TryGet(cacheKey, out UserStatsDto? cached) && cached is not null)
                return cached;

            var since = DateTime.UtcNow.AddDays(-days);

            var recentStats = await _context.PlaybackStats
                .Where(x => x.PlayedAt >= since)
                .ToListAsync();

            var totalTimeSeconds = recentStats.Sum(d => d.Duration.TotalSeconds);
            var totalTracks = recentStats.Count;

            var mostPlayed = recentStats
                .GroupBy(x => x.VideoId)
                .OrderByDescending(g => g.Count())
                .Select(g => new
                {
                    VideoId = g.Key,
                    Count = g.Count(),
                    Info = g.First()
                })
                .FirstOrDefault();

            var stats = new UserStatsDto
            {
                TotalListeningTime = TimeSpan.FromSeconds(totalTimeSeconds).ToString(@"hh\:mm\:ss"),
                TotalTracksPlayed = totalTracks,
                MostPlayed = mostPlayed == null ? null :
                    _mapper.Map<RecommendationDto>(
                        (mostPlayed.VideoId, mostPlayed.Info.Title, mostPlayed.Info.ThumbnailUrl, mostPlayed.Info.Duration)
                    )
            };

            if (stats.MostPlayed is not null)
            {
                stats.MostPlayed.PlayCount = mostPlayed!.Count;
            }


            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));               
            return stats;
        }
        private static IQueryable<PlaybackRankingDto> GetPlaybackRankingQuery(IQueryable<PlaybackStat> query)
        {
            return query
                .GroupBy(x => x.VideoId)
                .Select(g => new PlaybackRankingDto
                {
                    VideoId = g.Key,
                    Title = g.First().Title,
                    ThumbnailUrl = g.First().ThumbnailUrl,
                    Duration = g.First().Duration,
                    PlayCount = g.Count()
                })
                .OrderByDescending(x => x.PlayCount);
        }


        public Task<List<RecommendationDto>> GetSmartRecommendationsAsync(int limit = 10)
        {
            return _engine.GetSmartRecommendationsAsync(limit);
        }

        public Task<List<RecommendationDto>> GetSmartRecommendationsByTagsAsync(int limit = 10)
        {
            return _engine.GetSmartRecommendationsByTagsAsync(limit);
        }

        public Task<List<PlaybackRankingDto>> GetTop10MostPlayedLastMonthAsync()
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            return GetMostPlayedAsync(10, oneMonthAgo);
        }

    }
}
