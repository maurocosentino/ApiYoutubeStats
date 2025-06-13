public interface IHistoryService
{
    Task AddAsync(HistoryItem item);
    Task AddOrUpdateAsync(HistoryItem item);
    Task<bool> ExistsRecentlyAsync(string videoId, TimeSpan timeSpan);
    Task<List<HistoryItem>> GetRecentAsync(int count);
    Task<List<HistoryItem>> GetPagedAsync(int page, int pageSize);
    Task DeleteAllAsync();
}
