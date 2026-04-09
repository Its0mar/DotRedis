using codecrafters_redis.Models;

namespace codecrafters_redis.Commands;

public class LLen
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (args.Length < 1 || args[0] is not BulkString key)
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());

        if (!storage.TryGetValue(key, out var entry))
            return new Integer(0);
        
        if (entry.Expiration.HasValue && entry.Expiration.Value <= DateTime.UtcNow)
        {
            storage.Remove(key);
            return new Integer(0);
        }
        
        if (entry.Value is RespArray array) 
            return new Integer(array.Objects.Length);
        
        if (entry.Value is RespList list)
            return new Integer(list.Items.Count);

        return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());
    }
}