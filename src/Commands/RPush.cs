using codecrafters_redis.Models;
using codecrafters_redis.Services;
using RedisEntry = codecrafters_redis.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class RPush
{
    public static RespObject Execute(
        Span<RespObject> args,
        Dictionary<BulkString, RedisEntry> storage, 
        BlockingManager blockingManager)
    {
        if (args.Length < 2 || args[0] is not BulkString key)
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());
        
        string keyString = key.ToString();
    
        int added = 0;
        
        foreach (var item in args[1..]) 
        {
            if (blockingManager.ResolveWaiter(keyString, item)) 
            {
                added++;
                continue; 
            }

            var list = GetOrCreateList(key, storage); 
            list.Items.AddLast(item);

        }

        return new Integer(GetListLength(key, storage) + added);
    }
    
    private static RespList GetOrCreateList(BulkString key, Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (storage.TryGetValue(key, out var entry) && entry.Value is RespList existingList)
        {
            return existingList;
        }

        var newList = new RespList(new LinkedList<RespObject>());
        storage[key] = new RedisEntry(newList, null);
        return newList;
    }

    private static int GetListLength(BulkString key, Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (storage.TryGetValue(key, out var entry) && entry.Value is RespList list)
            return list.Items.Count;
        return 0;
    }
}