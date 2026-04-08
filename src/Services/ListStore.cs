using System.Collections.Concurrent;
using codecrafters_redis.Models;

namespace codecrafters_redis.Services;

public class ListStore
{
    private readonly ConcurrentDictionary<string, List<string>> _list = new();

    public int AddToList(string key, string value)
    {
        var list = _list.GetOrAdd(key, _ => new List<string>());

        lock (list)
        {
            list.Add(value);
            return list.Count;
        }
    }
}