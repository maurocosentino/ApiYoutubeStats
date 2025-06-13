using ApiYoutubeStats.DTOs;
using ApiYoutubeStats.Models;
using AutoMapper;

namespace ApiYoutubeStats.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<FavoriteCreateDto, Favorite>();
            CreateMap<Favorite, FavoriteDto>();
            CreateMap<HistoryItem, HistoryItemDto>();
            CreateMap<HistoryItem, HistoryItemDto>().ReverseMap();
            CreateMap<HistoryItemDto, HistoryItem>()
                .ForMember(dest => dest.PlayedAt, opt => opt.Ignore());
            CreateMap<PlaybackStat, PlaybackRankingDto>();
            CreateMap<PlaybackStat, RecommendationDto>();

            CreateMap<(string VideoId, string Title, string ThumbnailUrl, TimeSpan Duration), RecommendationDto>()
                .ForMember(dest => dest.VideoId, opt => opt.MapFrom(src => src.VideoId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailUrl))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration));

        }
    }
}
