public class HistoryItem
{
    public int Id { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }

    public string? ThumbnailUrl { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Tags { get; set; }

}

