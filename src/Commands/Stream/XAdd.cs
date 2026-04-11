using System.Text;
using codecrafters_redis.Core;
using codecrafters_redis.Models;

namespace codecrafters_redis.Commands.Stream;

public static class XAdd
{
    public static RespObject Execute(
        Span<RespObject> args,
        Dictionary<BulkString, RedisDatabase.RedisEntry> storage)
    {
        if (args.Length < 4 || (args.Length - 2) % 2 != 0)
            return new SimpleError("ERR wrong number of arguments"u8.ToArray());

        var key = (BulkString)args[0];
        var idStr = Encoding.UTF8.GetString(((BulkString)args[1]).Value);

        if (!storage.TryGetValue(key, out var entry) || entry.Value is not RespStream stream)
        {
            stream = new RespStream();
            storage[key] = new RedisDatabase.RedisEntry(stream, null);
        }
        
        if (!CheckId(idStr, stream, out var result))
        {
            if (string.IsNullOrWhiteSpace(result)) 
                return new SimpleError("ERR wrong type of arguments"u8.ToArray());
            else 
                return new SimpleError(Encoding.UTF8.GetBytes(result));
        }
        
        var values = new Dictionary<string, string>();

        for (int i = 2; i < args.Length; i += 2)
        {
            var k = Encoding.UTF8.GetString(((BulkString)args[i]).Value);
            var v = Encoding.UTF8.GetString(((BulkString)args[i + 1]).Value);
            values.Add(k, v);
        }
        
        stream.Entries.Add(new StreamEntry(idStr,  values));
        return new BulkString(Encoding.UTF8.GetBytes(idStr), idStr.Length);
    }

    private static bool CheckId(string newId, RespStream stream, out string error)
    {
        error = "";
        var parts = newId.Split('-');
        if (parts.Length != 2 || !long.TryParse(parts[0], out var msNew) || !long.TryParse(parts[1], out var snNew))
        {
            return false;
        }

        if ((msNew == 0 && snNew == 0) || snNew == 0)
        {
            error = "ERR The ID specified in XADD must be greater than 0-0";
            return false;
        }
        
        if (stream.Entries.Count == 0)
        {
            return true; 
        }
        
        var lastStreamEntry = stream.Entries.Last();
        var lastIdArray = lastStreamEntry.Id.Split('-');
        if (!long.TryParse(lastIdArray[0], out var msLast)) return false;
        if (!long.TryParse(lastIdArray[1], out var snLast)) return false;
        

        if (msLast > msNew)
        {
            error = "ERR The ID specified in XADD is equal or smaller than the target stream top item";
            return false;
        }

        if (msNew == msLast && snNew <= snLast)
        {
            error = "ERR The ID specified in XADD is equal or smaller than the target stream top item";
            return false;
        }

        error = newId;
        return true;

    }
}