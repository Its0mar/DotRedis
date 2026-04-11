using codecrafters_redis.Core;
using codecrafters_redis.Models;

namespace codecrafters_redis.Commands;

public static class LPop
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (args.Length == 0 || args[0] is not BulkString key) 
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());
        
        if (!storage.TryGetValue(key, out var entry))
            return BulkString.Null;

        if (entry.Expiration.HasValue && entry.Expiration.Value < DateTime.UtcNow)
        {
            storage.Remove(key);
            return BulkString.Null;
        }
        
        if (entry.Value is not RespList list)
            return new SimpleError("WRONGTYPE Operation against a key holding the wrong kind of value"u8.ToArray());
        
        if (list.Items.Count == 0)
        {
            storage.Remove(key);
            return BulkString.Null;
        }
        
        if (args.Length > 1)
        {
            if (args[1] is BulkString countBs && 
                System.Buffers.Text.Utf8Parser.TryParse(countBs.Value, out int count, out _))
            {
                var poppedList = LPopWithCount(list, count);
                if (list.Items.Count == 0) storage.Remove(key);
                
                return poppedList;
            }
        }
        
        var firstElement = list.Items.First!.Value;
        list.Items.RemoveFirst();
        
        if (list.Items.Count == 0)
        {
            storage.Remove(key);
        }

        return firstElement;
    }

    private static RespObject LPopWithCount(RespList list, int count)
    {
        if (count < 0) return new SimpleError("ERR value is out of range, must be positive"u8.ToArray());
        
        int actualPopCount = Math.Min(count, list.Items.Count);
        var poppedItems = new LinkedList<RespObject>();

        for (int i = 0; i < actualPopCount; i++)
        {
            var item = list.Items.First!.Value;    
            poppedItems.AddLast(item);
            list.Items.RemoveFirst();
        }

        return new RespList(poppedItems);

    }
    
}