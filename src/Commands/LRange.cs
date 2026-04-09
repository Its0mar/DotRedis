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
            return new RespArray([]);

        int listSize = 0;
        IEnumerable<RespObject> collection;

        if (redisEntry.Value is RespList respList)
        {
            listSize = respList.Items.Count;
            collection = respList.Items;
        }
        else if (redisEntry.Value is RespArray respArray)
        {
            listSize = respArray.Objects.Length;
            collection = respArray.Objects;
        }
        else
        {
            return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());
        }

        start = start >= 0 ? start : Math.Max(0, listSize + start);
        end = end >= 0 ? Math.Min(end, listSize - 1) : Math.Max(0, listSize + end);

        if (start > end || start >= listSize || start < 0)
            return new RespArray([]);

        var result = collection.Skip(start).Take(end - start + 1).ToArray();
    
        return new RespArray(result);
    }
}