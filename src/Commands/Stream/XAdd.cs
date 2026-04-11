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
        
        var values = new Dictionary<string, string>();

        for (int i = 2; i < args.Length; i += 2)
        {
            var k = Encoding.UTF8.GetString(((BulkString)args[i]).Value);
            var v = Encoding.UTF8.GetString(((BulkString)args[i + 1]).Value);
            values.Add(k, v);
        }
        
        if (!storage.TryGetValue(key, out var entry) || entry.Value is not RespStream stream)
        {
            stream = new RespStream();
            storage[key] = new RedisDatabase.RedisEntry(stream, null);
        }
        
        stream.Entries.Add(new StreamEntry(idStr,  values));
        return new BulkString(Encoding.UTF8.GetBytes(idStr), idStr.Length);
    }
}