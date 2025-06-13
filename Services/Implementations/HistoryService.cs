using Microsoft.EntityFrameworkCore;

public class HistoryService : IHistoryService
{
    private readonly AppDbContext _context;
    private readonly int _maxHistoryItems = 100;

    public HistoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(HistoryItem item)
    {
        var existing = await _context.HistoryItems
            .FirstOrDefaultAsync(h => h.VideoId == item.VideoId);

        if (existing != null)
        {
            existing.PlayedAt = DateTime.UtcNow;
            _context.HistoryItems.Update(existing);
        }
        else
        {
            _context.HistoryItems.Add(item);
        }

        await _context.SaveChangesAsync();

        var total = await _context.HistoryItems.CountAsync();
        if (total > _maxHistoryItems)
        {
            var toRemove = await _context.HistoryItems
                .OrderBy(h => h.PlayedAt)
                .Take(total - _maxHistoryItems)
                .ToListAsync();

            _context.HistoryItems.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsRecentlyAsync(string videoId, TimeSpan timeSpan)
    {
        var since = DateTime.UtcNow - timeSpan;
        return await _context.HistoryItems
            .AnyAsync(h => h.VideoId == videoId && h.PlayedAt >= since);
    }


    public async Task<List<HistoryItem>> GetRecentAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.HistoryItems
            .Where(x => x.PlayedAt >= cutoffDate)
            .OrderByDescending(x => x.PlayedAt)
            .ToListAsync();
    }

    public async Task<List<HistoryItem>> GetPagedAsync(int page, int pageSize)
    {
        return await _context.HistoryItems
            .OrderByDescending(h => h.PlayedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    public async Task AddOrUpdateAsync(HistoryItem item)
{
    var existing = await _context.HistoryItems
        .FirstOrDefaultAsync(h => h.VideoId == item.VideoId);

    if (existing != null)
    {
        existing.PlayedAt = DateTime.UtcNow;
        existing.Title = item.Title;
        existing.ThumbnailUrl = item.ThumbnailUrl;
        existing.Duration = item.Duration;
        _context.HistoryItems.Update(existing);
    }
    else
    {
        item.PlayedAt = DateTime.UtcNow;
        await _context.HistoryItems.AddAsync(item);
    }

    await _context.SaveChangesAsync();
}


    public async Task DeleteAllAsync()
    {
        var allItems = await _context.HistoryItems.ToListAsync();
        _context.HistoryItems.RemoveRange(allItems);
        await _context.SaveChangesAsync();
    }
}
