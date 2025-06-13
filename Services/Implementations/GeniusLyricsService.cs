using HtmlAgilityPack;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiYoutubeStats.Services.Implementations
{
    public class GeniusLyricsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken = "AYezpn6CqhW4cURyXkQoihyyhZ1wxoJDUOjSV1d41br6e6XGhIZcmuZh98_X0sBE";

        public GeniusLyricsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.genius.com/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<string?> GetLyricsAsync(string title, string artist)
        {
            var url = await SearchLyricsUrlAsync(title, artist);
            if (string.IsNullOrEmpty(url))
                return null;

            return await ExtractLyricsFromPage(url);
        }

        private async Task<string?> SearchLyricsUrlAsync(string title, string artist)
        {
            var query = $"{title} {artist}";
            var response = await _httpClient.GetAsync($"search?q={Uri.EscapeDataString(query)}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var hits = doc.RootElement.GetProperty("response").GetProperty("hits");

            if (hits.GetArrayLength() == 0)
                return null;

            var songPath = hits[0].GetProperty("result").GetProperty("path").GetString();
            return songPath != null ? $"https://genius.com{songPath}" : null;
        }

        private async Task<string?> ExtractLyricsFromPage(string url)
        {
            var html = await new HttpClient().GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var lyricsNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-lyrics-container='true']");

            if (lyricsNode == null)
                return null;

            return lyricsNode.InnerText.Trim();
        }
    }
}
