using ApiYoutubeStats.Mappings;
using ApiYoutubeStats.Services.Implementations;
using ApiYoutubeStats.Services.Interfaces;
using ApiYoutubeStats.Repositories;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode;
using ApiYoutubeStats.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios Scoped por solicitud
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IPlaybackManager, PlaybackManager>();
builder.Services.AddScoped<RecommendationEngine>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IPlaybackStatsRepository, PlaybackStatsRepository>();



builder.Services.AddAutoMapper(typeof(MappingProfile));

// Cliente Singleton para YoutubeExplode
builder.Services.AddSingleton<YoutubeClient>();

// HttpClients para servicios externos
builder.Services.AddHttpClient<ISearchService, YouTubeSearchService>();
builder.Services.AddHttpClient<GeniusLyricsService>();

builder.Services.AddScoped<IStatsCache, MemoryStatsCache>();
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddControllers();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}



if (Directory.Exists("wwwroot"))
{
    app.UseStaticFiles();
}



app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
