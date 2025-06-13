
public interface IPlaylistService
{
    Task<List<Playlist>> GetAllAsync();
    Task<Playlist?> GetByIdAsync(int id);
    Task<List<PlaylistItem>> GetItemsAsync(int playlistId, bool shuffle = false);
    Task<Playlist> CreateAsync(string name);
    Task<Playlist?> RenameAsync(int id, string newName);
    Task<bool> DeleteAsync(int id);
    Task<PlaylistItem?> AddItemAsync(int playlistId, PlaylistItemCreateDto dto);
    Task<bool> RemoveItemAsync(int itemId);
    Task<(List<PlaylistItem> items, bool repeat)> PlayPlaylistAsync(int playlistId, bool shuffle, bool repeat);

}
