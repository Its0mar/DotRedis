using codecrafters_redis.Models;
using RedisEntry = codecrafters_redis.Core.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class Get
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisEntry> storage)
    {
        if (args.Length is 0 || args[0] is not BulkString key)
            return new SimpleError("ERR Incorrect argument"u8.ToArray());

        if (!storage.TryGetValue(key, out var value))
            return BulkString.Null;

        if (value.Expiration is { } expiry && expiry <= DateTime.UtcNow)
        {
            storage.Remove(key);
            return  BulkString.Null;
        }

        return value.Value;
    }
}