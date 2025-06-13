using ApiYoutubeStats.Models;
using ApiYoutubeStats.Repositories;
using Microsoft.EntityFrameworkCore;

public class PlaybackStatsRepository : IPlaybackStatsRepository
{
    private readonly AppDbContext _context;

    public PlaybackStatsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlaybackStat>> GetStatsSinceAsync(DateTime? since = null)
    {
        var query = _context.PlaybackStats.AsQueryable();
        if (since.HasValue)
            query = query.Where(x => x.PlayedAt >= since.Value);
        return await query.ToListAsync();
    }

    public async Task<List<PlaybackStat>> GetRecentStatsAsync(int days)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await _context.PlaybackStats.Where(x => x.PlayedAt >= since).ToListAsync();
    }

    public async Task AddPlaybackAsync(PlaybackStat stat)
    {
        _context.PlaybackStats.Add(stat);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TimeSpan>> GetAllDurationsAsync()
    {
        return await _context.PlaybackStats.Select(x => x.Duration).ToListAsync();
    }
}
