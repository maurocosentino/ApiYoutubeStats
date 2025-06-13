using System.Diagnostics;

namespace ApiYoutubeStats.Utils
{
    public static class FfmpegHelper
    {
        public static async Task<bool> ConvertToMp3Async(string inputUrl, string outputPath, string ffmpegPath = "ffmpeg", int timeoutSeconds = 60)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{inputUrl}\" -vn -codec:a libmp3lame -qscale:a 0 \"{outputPath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                var errorOutput = await process.StandardError.ReadToEndAsync();
                var exited = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

                if (!exited || process.ExitCode != 0)
                {
                    Console.WriteLine($"FFmpeg error: {errorOutput}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción FFmpeg: {ex.Message}");
                return false;
            }
        }
    }
}
