
public interface IFavoriteService
{
    Task<List<FavoriteDto>> GetAllAsync();
    Task AddAsync(FavoriteCreateDto dto);
    Task RemoveAsync(string videoId);
    Task<bool> ExistsAsync(string videoId);
    Task<List<FavoriteArtistGroupDto>> GetFavoritesGroupedByArtistAsync();
}
