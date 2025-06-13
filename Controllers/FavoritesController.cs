using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _favoriteService.GetAllAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] FavoriteCreateDto dto)
    {
        await _favoriteService.AddAsync(dto);
        return Ok();
    }

    [HttpDelete("{videoId}")]
    public async Task<IActionResult> Remove(string videoId)
    {
        await _favoriteService.RemoveAsync(videoId);
        return NoContent();
    }

    [HttpGet("exists/{videoId}")]
    public async Task<IActionResult> Exists(string videoId)
    {
        var exists = await _favoriteService.ExistsAsync(videoId);
        return Ok(exists);
    }

    [HttpGet("group-by-artist")]
    public async Task<IActionResult> GroupByArtist()
    {
        var result = await _favoriteService.GetFavoritesGroupedByArtistAsync();
        return Ok(result);
    }
}
