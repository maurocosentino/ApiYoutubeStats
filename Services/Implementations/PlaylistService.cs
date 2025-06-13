
using Microsoft.EntityFrameworkCore;

public class PlaylistService : IPlaylistService
{
    private readonly AppDbContext _context;

    public PlaylistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Playlist>> GetAllAsync()
    {
        return await _context.Playlists
            .Include(p => p.Items)
            .ToListAsync();
    }

    public async Task<Playlist?> GetByIdAsync(int id)
    {
        return await _context.Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<PlaylistItem>> GetItemsAsync(int playlistId, bool shuffle = false)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null) return new List<PlaylistItem>();

        var items = playlist.Items.ToList();

        if (shuffle)
        {
            var rng = new Random();
            items = items.OrderBy(_ => rng.Next()).ToList();
        }

        return items;
    }

    public async Task<Playlist> CreateAsync(string name)
    {
        var playlist = new Playlist { Name = name };
        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();
        return playlist;
    }

    public async Task<Playlist?> RenameAsync(int id, string newName)
    {
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null) return null;

        playlist.Name = newName;
        await _context.SaveChangesAsync();
        return playlist;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (playlist == null) return false;

        _context.PlaylistItems.RemoveRange(playlist.Items);
        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PlaylistItem?> AddItemAsync(int playlistId, PlaylistItemCreateDto dto)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null) return null;

        var newItem = new PlaylistItem
        {
            PlaylistId = playlistId,
            Playlist = playlist,
            VideoId = dto.VideoId,
            Title = dto.Title,
            Duration = dto.Duration,
            ThumbnailUrl = dto.ThumbnailUrl
        };


        playlist.Items.Add(newItem);
        await _context.SaveChangesAsync();

        return newItem;
    }

    public async Task<(List<PlaylistItem> items, bool repeat)> PlayPlaylistAsync(int playlistId, bool shuffle, bool repeat)
    {
        var playlist = await _context.Playlists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null || !playlist.Items.Any())
            return (new List<PlaylistItem>(), repeat);

        var items = playlist.Items.ToList();

        if (shuffle)
        {
            var rng = new Random();
            items = items.OrderBy(_ => rng.Next()).ToList();
        }

        return (items, repeat);
    }


    public async Task<bool> RemoveItemAsync(int itemId)
    {
        var item = await _context.PlaylistItems.FindAsync(itemId);
        if (item == null) return false;

        _context.PlaylistItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
