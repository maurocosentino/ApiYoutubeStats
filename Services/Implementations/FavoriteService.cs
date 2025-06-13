
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class FavoriteService : IFavoriteService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public FavoriteService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FavoriteDto>> GetAllAsync()
    {
        var favorites = await _context.Favorites
            .OrderByDescending(f => f.Id)
            .ToListAsync();

        return _mapper.Map<List<FavoriteDto>>(favorites);
    }


    public async Task AddAsync(FavoriteCreateDto dto)
    {
        var exists = await _context.Favorites.AnyAsync(f => f.VideoId == dto.VideoId);
        if (exists) return;

        var fav = _mapper.Map<Favorite>(dto);

        _context.Favorites.Add(fav);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(string videoId)
    {
        var fav = await _context.Favorites.FirstOrDefaultAsync(f => f.VideoId == videoId);
        if (fav != null)
        {
            _context.Favorites.Remove(fav);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string videoId)
    {
        return await _context.Favorites.AnyAsync(f => f.VideoId == videoId);
    }

    public async Task<List<FavoriteArtistGroupDto>> GetFavoritesGroupedByArtistAsync()
    {
        return await _context.Favorites
            .GroupBy(f => f.Artist)
            .Select(g => new FavoriteArtistGroupDto
            {
                Artist = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .ToListAsync();
    }
}
