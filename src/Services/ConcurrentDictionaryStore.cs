using System.Collections.Concurrent;
using codecrafters_redis.Enums;
using codecrafters_redis.Models;

namespace codecrafters_redis.Services;

public class ConcurrentDictionaryStore
{
    private readonly ConcurrentDictionary<string, StoredValue> _data = new();

    public void Set(string key, string value, DateTime? expiry = null)
    {
        _data[key] = new StoredValue { Value = value, Expiry = expiry };
    }

    public string? GetString(string key)
    {
        if (!TryGetValidEntry(key, out var storedValue)) return null;
        
        if (storedValue.Type != DataType.String)  return null;

        return (string)storedValue.Value;
    }

    public List<string>? GetList(string key)
    {
        if (!TryGetValidEntry(key, out var storedValue)) return null;
        if (storedValue.Type != DataType.List) return null;
        
        return (List<string>)storedValue.Value;
    }

    public int AddToList(string key, IEnumerable<string> value)
    {
        var entry = _data.GetOrAdd(key, _ => new StoredValue 
        { 
            Type = DataType.List, 
            Value = new List<string>(), 
            Expiry = null 
        });

        if (entry.Type != DataType.List) return -1; 

        var list = (List<string>)entry.Value;
        
        lock (list) 
        {
            list.AddRange(value);
            return list.Count;
        }
    }

    private bool TryGetValidEntry(string key, out StoredValue entry)
    {
        if (!_data.TryGetValue(key, out entry)) return false;

        if (entry.Expiry is { } expiry && expiry <= DateTime.UtcNow)
        {
            _data.TryRemove(key, out _);
            return false;
        }

        return true;
    }
}