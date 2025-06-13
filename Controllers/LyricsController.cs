using ApiYoutubeStats.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiYoutubeStats.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LyricsController : ControllerBase
    {
        private readonly GeniusLyricsService _lyricsService;

        public LyricsController(GeniusLyricsService lyricsService)
        {
            _lyricsService = lyricsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLyrics([FromQuery] string title, [FromQuery] string artist)
        {
            var lyrics = await _lyricsService.GetLyricsAsync(title, artist);

            if (string.IsNullOrEmpty(lyrics))
                return NotFound("Letra no encontrada.");

            return Ok(new { title, artist, lyrics });
        }
    }
}
