public interface IStatsCache
{
    bool TryGet<T>(string key, out T? value);
    void Set<T>(string key, T value, TimeSpan duration);
    void Remove(string key);
}
