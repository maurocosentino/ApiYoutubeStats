using ApiYoutubeStats.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<HistoryItem> HistoryItems { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<PlaybackStat> PlaybackStats { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistItem> PlaylistItems { get; set; }
    public DbSet<PlaybackHistory> PlaybackHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
       
        modelBuilder.Entity<HistoryItem>().HasKey(h => h.Id);
        modelBuilder.Entity<HistoryItem>().HasIndex(h => h.PlayedAt);
        modelBuilder.Entity<Favorite>().HasKey(f => f.Id);
        modelBuilder.Entity<PlaybackStat>().HasKey(s => s.Id);
        modelBuilder.Entity<Playlist>().HasKey(p => p.Id);
        modelBuilder.Entity<PlaylistItem>().HasKey(pi => pi.Id);

        modelBuilder.Entity<PlaylistItem>()
            .HasOne(p => p.Playlist)
            .WithMany(p => p.Items)
            .HasForeignKey(p => p.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HistoryItem>()
        .Property(h => h.Duration)
        .HasConversion(
            v => v.HasValue ? v.Value.Ticks : (long?)null,
            v => v.HasValue ? TimeSpan.FromTicks(v.Value) : (TimeSpan?)null
        );

    }
}
