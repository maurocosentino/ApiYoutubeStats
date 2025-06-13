using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/search")]
public class YoutubeSearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<YoutubeSearchController> _logger;

    public YoutubeSearchController(ISearchService searchService, ILogger<YoutubeSearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }


    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {

        if (string.IsNullOrWhiteSpace(q))
        {
            _logger.LogWarning("Intento de búsqueda con query vacío o nulo");
            return BadRequest("Query is required");
        }

        try
        {
            _logger.LogInformation("Buscando videos para la query: {Query}", q);
            var results = await _searchService.SearchYoutubeAsync(q);
            _logger.LogInformation("Se obtuvieron {Count} resultados para la query: {Query}", results.Count, q);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar videos para la query: {Query}", q);
            return StatusCode(500, "Error interno del servidor");
        }
    }



    [HttpGet("playlist/{playlistId}")]
    public async Task<IActionResult> GetPlaylist(string playlistId)
    {
        try
        {
            var result = await _searchService.GetPlaylistInfoAsync(playlistId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de la playlist: {PlaylistId}", playlistId);
            return StatusCode(500, "Error al obtener la playlist.");
        }
    }

}
