using ApiYoutubeStats.DTOs;
using ApiYoutubeStats.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiYoutubeStats.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;
        private readonly ILogger<StatsController> _logger;
        private readonly IPlaybackManager _playbackManager;

        public StatsController(IStatsService statsService, ILogger<StatsController> logger, IPlaybackManager playbackManager)
        {
            _statsService = statsService;
            _logger = logger;
            _playbackManager = playbackManager;
        }

        [HttpPost("playback")]
        public async Task<IActionResult> RegisterPlayback([FromBody] PlaybackRegisterDto dto)
        {
            try
            {
                await _playbackManager.RegisterPlaybackAsync(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering playback.");
                return StatusCode(500, "Failed to register playback.");
            }
        }



        [HttpGet("most-played")]
        public async Task<IActionResult> GetMostPlayed()
        {
            try
            {
                var result = await _statsService.GetMostPlayedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most played videos.");
                return StatusCode(500, "Failed to get most played videos.");
            }
        }

        [HttpGet("most-played-month")]
        public async Task<IActionResult> GetMostPlayedLastMonth()
        {
            try
            {
                var result = await _statsService.GetTop10MostPlayedLastMonthAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most played videos last month.");
                return StatusCode(500, "Failed to get most played videos last month.");
            }
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTopPlayed([FromQuery] int limit = 10)
        {
            try
            {
                var result = await _statsService.GetTopPlayedAsync(limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top played videos.");
                return StatusCode(500, "Failed to get top played videos.");
            }
        }

        [HttpGet("listening-time")]
        public async Task<IActionResult> GetListeningTime()
        {
            try
            {
                var time = await _statsService.GetTotalListeningTimeAsync();
                return Ok(new
                {
                    time.TotalSeconds,
                    Formatted = time.ToString(@"hh\:mm\:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total listening time.");
                return StatusCode(500, "Failed to get total listening time.");
            }
        }

        [HttpGet("userstats")]
        public async Task<IActionResult> GetUserStats([FromQuery] int days = 7)
        {
            try
            {
                var stats = await _statsService.GetUserStatsAsync(days);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user stats.");
                return StatusCode(500, "Failed to get user stats.");
            }
        }

    }
}
