using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

public class YouTubeSearchService : ISearchService
{
    private readonly ILogger<YouTubeSearchService> _logger;
    private readonly HttpClient _http;
    private readonly string[] _apiKeys;
    private readonly IMemoryCache _cache;

    private int _currentKeyIndex = 0;

    public YouTubeSearchService(
        IConfiguration config,
        HttpClient http,
        ILogger<YouTubeSearchService> logger,
        IMemoryCache cache)
    {
        _http = http;
        _logger = logger;
        _cache = cache;

        _apiKeys = config.GetSection("YouTube:ApiKeys").Get<string[]>()
                   ?? throw new Exception("No se encontraron claves API.");
    }

    public async Task<List<YouTubeSearchResultDto>> SearchYoutubeAsync(string query, int maxResults)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("La query no puede estar vacía.");

        string cacheKey = $"youtube-search:{query.ToLower()}";
        if (_cache.TryGetValue(cacheKey, out List<YouTubeSearchResultDto>? cached) && cached is not null)
        {
            _logger.LogInformation("Usando caché para '{Query}'", query);
            return cached;
        }


        for (int i = 0; i < _apiKeys.Length; i++)
        {
            string apiKey = _apiKeys[_currentKeyIndex];
            string url = $"https://www.googleapis.com/youtube/v3/search?part=snippet&type=video&maxResults={maxResults}&q={Uri.EscapeDataString(query)}&key={apiKey}";

            try
            {
                var response = await _http.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Error HTTP {StatusCode} al buscar '{Query}' con clave #{Index}. Contenido: {Json}",
                        response.StatusCode, query, _currentKeyIndex, json);

                    if ((int)response.StatusCode == 403 && json.Contains("quotaExceeded"))
                    {
                        _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Length;
                        continue;
                    }

                    throw new Exception("Error en búsqueda: " + json);
                }

                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("items", out var items))
                {
                    _logger.LogWarning("La respuesta de YouTube para '{Query}' no contiene 'items'. JSON: {Json}", query, json);
                    return new List<YouTubeSearchResultDto>();
                }

                var results = new List<YouTubeSearchResultDto>();

                foreach (var item in items.EnumerateArray())
                {
                    if (!item.TryGetProperty("id", out var idProp)) continue;
                    if (!idProp.TryGetProperty("videoId", out var videoIdProp)) continue;
                    var id = videoIdProp.GetString();

                    if (!item.TryGetProperty("snippet", out var snippet)) continue;
                    var title = snippet.GetProperty("title").GetString() ?? "";
                    var thumbnailUrl = snippet.GetProperty("thumbnails").GetProperty("high").GetProperty("url").GetString() ?? "";
                    var channelTitle = snippet.GetProperty("channelTitle").GetString() ?? "";

                    results.Add(new YouTubeSearchResultDto
                    {
                        VideoId = id ?? "",
                        Title = title,
                        ThumbnailUrl = thumbnailUrl,
                        ChannelTitle = channelTitle
                    });
                }

                await AddDurationsAsync(results, apiKey);

                _cache.Set(cacheKey, results, TimeSpan.FromMinutes(30));
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al buscar '{Query}' con clave #{Index}. Probando siguiente clave...", query, _currentKeyIndex);
                _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Length;
            }
        }

        throw new Exception("Todas las claves API fallaron.");
    }
    private async Task AddDurationsAsync(List<YouTubeSearchResultDto> results, string apiKey)
    {
        if (results == null || results.Count == 0) return;

        var ids = string.Join(",", results.Select(r => r.VideoId));
        var url = $"https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id={ids}&key={apiKey}";

        var response = await _http.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // Detectar cuota excedida
            if ((int)response.StatusCode == 403 && json.Contains("quotaExceeded"))
            {
                throw new Exception("Quota exceeded");
            }
            _logger.LogWarning("Error al obtener duraciones: {Status} - {Content}", response.StatusCode, json);
            return;
        }

        using var doc = JsonDocument.Parse(json);

        foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
        {
            var id = item.GetProperty("id").GetString();
            var iso = item.GetProperty("contentDetails").GetProperty("duration").GetString();

            var video = results.FirstOrDefault(r => r.VideoId == id);
            if (video != null && iso != null)
            {
                video.Duration = ConvertFromIsoDuration(iso);
            }
        }
    }


    private string ConvertFromIsoDuration(string iso)
    {
        try
        {
            var ts = System.Xml.XmlConvert.ToTimeSpan(iso);
            return ts.Hours > 0
                ? $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes}:{ts.Seconds:D2}";
        }
        catch
        {
            return "";
        }
    }
}
