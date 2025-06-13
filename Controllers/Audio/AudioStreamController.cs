using ApiYoutubeStats.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

[ApiController]
[Route("api/stream")]
public class AudioStreamController : ControllerBase
{
    private readonly YoutubeClient _youtube;
    private readonly IHistoryService _historyService;
    private readonly ILogger<AudioStreamController> _logger;
    //private readonly IStatsService _statsService;

    public AudioStreamController(
        YoutubeClient youtube,
        IHistoryService historyService,
        IStatsService statsService,
        ILogger<AudioStreamController> logger)
    {
        _youtube = youtube;
        _historyService = historyService;
        //_statsService = statsService;
        _logger = logger;
    }

    [HttpGet("{videoId}")]
    public async Task<IActionResult> StreamAudio(string videoId)
    {
        try
        {
            var video = await _youtube.Videos.GetAsync(videoId);
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (string.IsNullOrWhiteSpace(videoId))
                return BadRequest("El videoId es obligatorio.");

            if (audioStreamInfo == null)
                return NotFound("No se encontraron streams de audio para este video.");

            var stream = await _youtube.Videos.Streams.GetAsync(audioStreamInfo);

            var recentlyPlayed = await _historyService.ExistsRecentlyAsync(videoId, TimeSpan.FromHours(1));

            await _historyService.AddOrUpdateAsync(new HistoryItem
            {
                VideoId = videoId,
                Title = video.Title,
                ThumbnailUrl = video.Thumbnails?.FirstOrDefault()?.Url ?? "",
                Duration = video.Duration ?? TimeSpan.Zero
            });


            var mime = audioStreamInfo.Container.Name == "mp4" ? "audio/mp4" : "audio/webm";
            return File(stream, mime, enableRangeProcessing: true);
        }
        catch (YoutubeExplode.Exceptions.VideoUnavailableException)
        {
            return NotFound("Audio/Video no disponible o privado.");
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Error al conectar con YouTube. Intenta más tarde.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en StreamAudio");
            return BadRequest($"Error inesperado: {ex.Message}");
        }
    }
}
