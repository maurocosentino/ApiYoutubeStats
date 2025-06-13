using ApiYoutubeStats.Models;

namespace ApiYoutubeStats.Repositories
{
    public interface IPlaybackStatsRepository
    {
        Task<List<PlaybackStat>> GetStatsSinceAsync(DateTime? since = null);
        Task<List<PlaybackStat>> GetRecentStatsAsync(int days);
        Task AddPlaybackAsync(PlaybackStat stat);
        Task<List<TimeSpan>> GetAllDurationsAsync();
    }

}
