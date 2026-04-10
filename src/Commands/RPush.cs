using codecrafters_redis.Models;
using RedisEntry = codecrafters_redis.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class RPush
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisEntry> storage)
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
            list = new LinkedList<RespObject>();
            storage[key] = new RedisEntry(new RespList(list), null);
        }
        
        foreach (var item in args[1..])
        {
            list.AddLast(item);
        }

        return new Integer(list.Count);
    }

}