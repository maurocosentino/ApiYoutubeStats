using ApiYoutubeStats.DTOs;
using ApiYoutubeStats.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IPlaybackManager _playbackManager;
    private readonly IHistoryService _historyService;
    private readonly IMapper _mapper;

    public HistoryController(IPlaybackManager playbackManager, IHistoryService historyService, IMapper mapper)
    {
        _playbackManager = playbackManager;
        _historyService = historyService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] HistoryItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.VideoId))
            return BadRequest("VideoId es requerido.");

        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title es requerido.");

        var playbackDto = new PlaybackRegisterDto
        {
            VideoId = dto.VideoId,
            Title = dto.Title,
            ThumbnailUrl = dto.ThumbnailUrl ?? "",
            Duration = dto.Duration ?? TimeSpan.FromSeconds(0)
        };

        await _playbackManager.RegisterPlaybackAsync(playbackDto);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 50;

        var history = await _historyService.GetPagedAsync(page, pageSize);
        var dtos = _mapper.Map<List<HistoryItemDto>>(history);

        return Ok(dtos);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10)
    {
        if (count <= 0 || count > 100) count = 10;

        var history = await _historyService.GetPagedAsync(1, count);
        var dtos = _mapper.Map<List<HistoryItemDto>>(history);

        return Ok(dtos);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _historyService.DeleteAllAsync();
        return NoContent();
    }
}
