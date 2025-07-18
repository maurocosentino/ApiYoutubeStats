﻿using ApiYoutubeStats.Configurations;
using ApiYoutubeStats.Mappings;
using ApiYoutubeStats.Services.Implementations;
using ApiYoutubeStats.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using YoutubeExplode;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();
builder.Services.Configure<YouTubeSettings>(builder.Configuration.GetSection("YouTube"));

//#if DEBUG
//var apiKeys = builder.Configuration.GetSection("YouTube:ApiKeys").Get<List<string>>();
//foreach (var key in apiKeys)
//{
//    Console.WriteLine($"YouTube API Key: {key}");
//}
//#endif
#if DEBUG
var apiKeys = builder.Configuration.GetSection("YouTube:ApiKeys").Get<List<string>>();

if (apiKeys != null && apiKeys.Any())
{
    foreach (var key in apiKeys)
    {
        Console.WriteLine($"YouTube API Key: {key}");
    }
}
else
{
    Console.WriteLine("No YouTube API Keys found in configuration.");
}
#endif


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IStatsCache, MemoryStatsCache>();
builder.Services.AddScoped<RecommendationEngine>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<IPlaybackManager, PlaybackManager>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();



builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddSingleton<YoutubeClient>();
builder.Services.AddHttpClient<ISearchService, YouTubeSearchService>();

builder.Services.AddHttpClient<GeniusLyricsService>();

builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

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
