using Microsoft.EntityFrameworkCore;
using ApiYoutubeStats.Models;
using ApiYoutubeStats.Services.Interfaces;
using ApiYoutubeStats.DTOs;

namespace ApiYoutubeStats.Services.Implementations
{
    public class PlaybackManager : IPlaybackManager
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly TimeSpan _minPlaybackInterval;



        public PlaybackManager(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            var seconds = _config.GetValue("Playback:MinIntervalSeconds", 30);
            _minPlaybackInterval = TimeSpan.FromSeconds(seconds);
        }


        public async Task RegisterPlaybackAsync(PlaybackRegisterDto dto)
        {
            var now = DateTime.UtcNow;

            var lastPlayback = await _context.PlaybackStats
                .Where(x => x.VideoId == dto.VideoId)
                .OrderByDescending(x => x.PlayedAt)
                .Select(x => x.PlayedAt)
                .FirstOrDefaultAsync();

            if (lastPlayback != default && now - lastPlayback < _minPlaybackInterval)
            {
                return;
            }

            var tags = string.IsNullOrWhiteSpace(dto.Tags) ? InferTags(dto.Title) : dto.Tags;


            var history = new HistoryItem
            {
                VideoId = dto.VideoId,
                Title = dto.Title,
                ThumbnailUrl = dto.ThumbnailUrl,
                Duration = dto.Duration,
                PlayedAt = now,
                Tags = tags
            };
            _context.HistoryItems.Add(history);

            var stat = new PlaybackStat
            {
                VideoId = dto.VideoId,
                Title = dto.Title,
                ThumbnailUrl = dto.ThumbnailUrl,
                Duration = dto.Duration,
                PlayedAt = now,
                Tags = tags
            };
            _context.PlaybackStats.Add(stat);

            await _context.SaveChangesAsync();
        }


        private string InferTags(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "otros";

            title = title.ToLower();
            if (title.Contains("lofi")) return "lofi,chill";
            if (title.Contains("piano")) return "instrumental,piano";
            return "otros";
        }

    }
}
