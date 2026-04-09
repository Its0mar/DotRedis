using codecrafters_redis.Models;
using RedisEntry = codecrafters_redis.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class RPush
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisEntry> storage)
    {
        if (args.Length < 2 || args[0] is not BulkString key)
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());
        
        List<RespObject> list;

        if (storage.TryGetValue(key, out var entry))
        {
            if (entry.Value is not RESPArray respArray)
            {
                return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());
            }
            list = respArray.Objects.ToList(); 
        }
        else
        {
            list = new List<RespObject>();
        }

        foreach (var item in args[1..])
        {
            list.Add(item);
        }

        var updatedArray = new RESPArray(list.ToArray());
        storage[key] = new RedisEntry(updatedArray, null);

        return new Integer(list.Count);
    }

}