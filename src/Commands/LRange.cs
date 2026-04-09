using codecrafters_redis.Models;
using RedisEntry = codecrafters_redis.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class LRange
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisEntry> storage)
    {
        if (args.Length < 3 || args[0] is not BulkString keyString)
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());
        
        if (args[1] is not BulkString startBs || args[2] is not BulkString endBs ||
            !System.Buffers.Text.Utf8Parser.TryParse(startBs.Value, out int start, out _) ||
            !System.Buffers.Text.Utf8Parser.TryParse(endBs.Value, out int end, out _))
            return new SimpleError("ERR value is not an integer"u8.ToArray());
        
        if (!storage.TryGetValue(keyString, out var redisEntry))
            return new RESPArray([]);
        
        if (redisEntry.Value is not RESPArray list)
            return new RESPArray([]);
        
        if (redisEntry.Expiration.HasValue && redisEntry.Expiration.Value < DateTime.UtcNow)
        {
            storage.Remove(keyString);
            return new RESPArray([]);
        }

        var listSize = list.Objects.Length;
        
        start = start >= 0 ? start : Math.Max(0, listSize + start);
        end = end >= 0 ? Math.Min(end, listSize - 1) : Math.Max(0, listSize + end);
        
        if (start > end || start >= listSize|| start < 0)
            return new RESPArray([]);
        
        var result = new RespObject[end - start + 1];
        for (int i = start; i <= end; i++)
            result[i - start] = list[i];
        
        return new RESPArray(result);
    }
}