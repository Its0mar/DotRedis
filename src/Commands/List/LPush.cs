using codecrafters_redis.Core;
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
            var respList = new RespList(list);
            storage[key] = new RedisDatabase.RedisEntry(respList, null);
        }

        foreach (var item in args[1..])
        {
            list.AddFirst(item);
        }

        return new Integer(list.Count);
    }
}