using codecrafters_redis.Models;

namespace codecrafters_redis.Commands;

public static class LPop
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (args.Length == 0 || args[0] is not BulkString key) 
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());

        if (!storage.TryGetValue(key, out var entry))
            return BulkString.Null;

        if (entry.Expiration.HasValue && entry.Expiration.Value < DateTime.UtcNow)
        {
            storage.Remove(key);
            return BulkString.Null;
        }
        
        if (entry.Value is not RespList list)
            return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());
        
        if (list.Items.Count == 0)
        {
            storage.Remove(key);
            return BulkString.Null;
        }
        
        var firstElement = list.Items.First!.Value;
        list.Items.RemoveFirst();
        
        if (list.Items.Count == 0)
        {
            storage.Remove(key);
        }

        return firstElement;
    }
    
}