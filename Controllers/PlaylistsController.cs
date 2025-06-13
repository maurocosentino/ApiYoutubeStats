using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _service;

    public PlaylistsController(IPlaylistService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Playlist>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var playlists = await _service.GetAllAsync();
        return Ok(playlists);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Playlist), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var playlist = await _service.GetByIdAsync(id);
        if (playlist == null) return NotFound();
        return Ok(playlist);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Playlist), 201)]
    public async Task<IActionResult> Create([FromBody] string name)
    {
        var playlist = await _service.CreateAsync(name);
        return CreatedAtAction(nameof(GetById), new { id = playlist.Id }, playlist);
    }

    [HttpPut("{id}/rename")]
    [ProducesResponseType(typeof(Playlist), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Rename(int id, [FromBody] string newName)
    {
        var updated = await _service.RenameAsync(id, newName);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/items")]
    [ProducesResponseType(typeof(List<PlaylistItem>), 200)]
    public async Task<IActionResult> GetItems(int id, [FromQuery] bool shuffle = false)
    {
        var items = await _service.GetItemsAsync(id, shuffle);
        return Ok(items);
    }

    [HttpPost("{id}/items")]
    [ProducesResponseType(typeof(PlaylistItem), 201)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddItem(int id, [FromBody] PlaylistItemCreateDto dto)
    {
        var item = await _service.AddItemAsync(id, dto);
        if (item == null) return NotFound();
        return Created("", item);
    }

    [HttpDelete("items/{itemId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var result = await _service.RemoveItemAsync(itemId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/play")]
    [ProducesResponseType(typeof(List<PlaylistItem>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Play(int id, [FromQuery] bool shuffle = false, [FromQuery] bool repeat = false)
    {
        var (items, _) = await _service.PlayPlaylistAsync(id, shuffle, repeat);
        if (items.Count == 0) return NotFound();
        return Ok(items);
    }
}
