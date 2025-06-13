using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Playlists;
using ApiYoutubeStats.Utils;

namespace backend.Controllers.Audio
{
    [ApiController]
    [Route("api/[controller]")]
    public class YoutubeDownloadController : ControllerBase
    {
        private readonly YoutubeClient _youtube;

        public YoutubeDownloadController(YoutubeClient youtube)
        {
            _youtube = youtube;
        }


        [HttpGet("single")]
        public async Task<IActionResult> DownloadSingle([FromQuery] string videoId)
        {
            try
            {
                var video = await _youtube.Videos.GetAsync(videoId);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
                var stream = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                if (stream == null)
                    return BadRequest("No se encontró el stream de audio.");

                var safeTitle = Regex.Replace(video.Title, @"[^\w\s-]", "");
                var tempFile = Path.GetTempFileName();
                var outputFile = Path.ChangeExtension(tempFile, ".mp3");

                var success = await FfmpegHelper.ConvertToMp3Async(stream.Url, outputFile, "ffmpeg");

                if (!success) return StatusCode(500, "Falló la conversión");

                var bytes = await System.IO.File.ReadAllBytesAsync(outputFile);
                System.IO.File.Delete(outputFile);

                return File(bytes, "audio/mpeg", $"{safeTitle}.mp3");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("playlist")]
        public async Task<IActionResult> DownloadPlaylist([FromQuery] string playlistId, [FromQuery] int max = 10)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var playlist = await _youtube.Playlists.GetAsync(playlistId);
                var videos = new List<PlaylistVideo>();
                var errores = new List<string>();


                await foreach (var video in _youtube.Playlists.GetVideosAsync(playlist.Id))
                {
                    if (videos.Count >= max) break;
                    videos.Add(video);
                }

                foreach (var video in videos)
                {
                    try
                    {
                        var manifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);
                        var stream = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                        if (stream == null)
                        {
                            errores.Add($"[Sin stream] {video.Title}");
                            continue;
                        }

                        var safeTitle = Regex.Replace(video.Title, @"[^\w\s-]", "_");
                        var outputPath = Path.Combine(tempDir, $"{safeTitle}.mp3");

                        var success = await FfmpegHelper.ConvertToMp3Async(stream.Url, outputPath, "ffmpeg");
                        if (!success)
                            errores.Add($"[FFmpeg fallo] {video.Title}");
                    }
                    catch (Exception ex)
                    {
                        errores.Add($"[Error] {video.Title} - {ex.Message}");
                    }
                }

                if (errores.Any())
                {
                    var errorFilePath = Path.Combine(tempDir, "errores.txt");
                    await System.IO.File.WriteAllLinesAsync(errorFilePath, errores);
                }

                var safePlaylistTitle = Regex.Replace(playlist.Title, @"[^\w\s-]", "_");
                var zipPath = Path.Combine(Path.GetTempPath(), $"{safePlaylistTitle}.zip");

                if (System.IO.File.Exists(zipPath)) System.IO.File.Delete(zipPath);

                ZipFile.CreateFromDirectory(tempDir, zipPath);
                var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);

                Directory.Delete(tempDir, true);
                System.IO.File.Delete(zipPath);

                return File(zipBytes, "application/zip", $"{playlist.Title}.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error general: {ex.Message}");
            }
        }
    }
}
