using codecrafters_redis.Core;
using codecrafters_redis.Models;

namespace codecrafters_redis.Commands.Types;

public static class Type
{
    public static RespObject Execute(
        Span<RespObject> args,
        Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (args.Length == 0)
            return new SimpleError("ERR wrong number of arguments for 'blpop' command"u8.ToArray());
            
        if (args[0] is not BulkString bulkKey)
                return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());

        if (!storage.TryGetValue(bulkKey, out var entry))
        {
            return new SimpleString("none"u8.ToArray());
        }
        
        if (entry.Value is BulkString)
            return new SimpleString("string"u8.ToArray());

        if (entry.Value is RespList)
            return new SimpleString("list"u8.ToArray());
        
        if (entry.Value is RespStream)
            return new SimpleString("list"u8.ToArray());
        
        return new SimpleString("none"u8.ToArray());
    }
}