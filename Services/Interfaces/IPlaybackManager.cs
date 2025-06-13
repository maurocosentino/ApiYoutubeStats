using ApiYoutubeStats.DTOs;

namespace ApiYoutubeStats.Services.Interfaces
{
    public interface IPlaybackManager
    {
        Task RegisterPlaybackAsync(PlaybackRegisterDto dto);
    }
}
