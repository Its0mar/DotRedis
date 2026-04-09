using codecrafters_redis.Models;

namespace codecrafters_redis.Commands;

public static class LPush
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (args.Length < 2 || args[0] is not BulkString key)
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());

        LinkedList<RespObject> list;

        if (storage.TryGetValue(key, out var entry))
        {
            if (entry.Value is not RespList respList)
            {
                return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());
            }
            list = respList.Items;
        }
        else
        {
            list = [];
        }

        foreach (var item in args[1..])
        {
            list.AddFirst(item);
        }

        var redisEntry = new RespList(list);
        storage[key] = new RedisDatabase.RedisEntry(redisEntry, null);
        return new Integer(list.Count);
    }
}