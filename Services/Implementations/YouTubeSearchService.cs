using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ApiYoutubeStats.Configurations;
using Microsoft.Extensions.Options;
using YoutubeExplode.Common;

public class YouTubeSearchService : ISearchService
{
    private readonly ILogger<YouTubeSearchService> _logger;
    private readonly HttpClient _http;
    private readonly string[] _apiKeys;
    private readonly IMemoryCache _cache;

    private int _currentKeyIndex = 0;

    public YouTubeSearchService(
    IOptions<YouTubeSettings> youtubeSettings,
    HttpClient http,
    ILogger<YouTubeSearchService> logger,
    IMemoryCache cache)
    {
        _http = http;
        _logger = logger;
        _cache = cache;

        _apiKeys = youtubeSettings.Value.ApiKeys ?? Array.Empty<string>();

        _logger.LogInformation("Cantidad de claves API cargadas: {Count}", _apiKeys.Length);

        if (_apiKeys.Length == 0)
        {
            _logger.LogWarning("No se encontraron claves API. Se usará YoutubeExplode como alternativa.");
        }

    }

    public async Task<List<YouTubeSearchResultDto>> SearchYoutubeAsync(string query, int maxResults)
    {
        //Funciona igual sin las claves de la API
        if (_apiKeys.Length == 0)
        {
            _logger.LogWarning("No hay claves API configuradas. Usando solo YoutubeExplode.");
            return await SearchWithYoutubeExplode(query, maxResults);
        }

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

            //Si se vence una clave busca la siguiente y si fallan todas usa SearchWithYoutubeExplode
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al buscar '{Query}' con clave #{Index}. Probando siguiente clave...", query, _currentKeyIndex);
                _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Length;
            }
        }

        _logger.LogWarning("Todas las claves han sido probadas o fallaron. Usando YoutubeExplode como alternativa...");
        var explodeResults = await SearchWithYoutubeExplode(query, maxResults);

        if (explodeResults == null || explodeResults.Count == 0)
        {
            _logger.LogError("YoutubeExplode también falló al buscar '{Query}'", query);
            throw new Exception("No se pudieron obtener resultados desde ninguna fuente.");
        }

        _cache.Set(cacheKey, explodeResults, TimeSpan.FromMinutes(15));
        return explodeResults;

    }
    private static async Task<List<YouTubeSearchResultDto>> SearchWithYoutubeExplode(string query, int maxResults)
    {
        var youtube = new YoutubeExplode.YoutubeClient();
        var searchResults = await youtube.Search.GetVideosAsync(query).CollectAsync(maxResults);

        return searchResults.Select(video => new YouTubeSearchResultDto
        {
            VideoId = video.Id.Value,
            Title = video.Title,
            ThumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url,
            ChannelTitle = video.Author?.ChannelTitle ?? "",
            Duration = video.Duration.HasValue
                ? (video.Duration.Value.Hours > 0
                    ? $"{video.Duration.Value.Hours}:{video.Duration.Value.Minutes:D2}:{video.Duration.Value.Seconds:D2}"
                    : $"{video.Duration.Value.Minutes}:{video.Duration.Value.Seconds:D2}")
                : ""
        }).ToList();
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
            // Detecta cuota excedida
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


    private static string ConvertFromIsoDuration(string iso)
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
