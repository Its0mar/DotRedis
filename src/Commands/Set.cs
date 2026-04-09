using System.Text;
using codecrafters_redis.Models;
using RedisEntry = codecrafters_redis.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class Set
{
    public static RespObject Execute(Span<RespObject> args, Dictionary<BulkString, RedisEntry> storage)
    {
        if (args.Length < 2)
            return new SimpleError("ERR SET requires at least 2 arguments"u8.ToArray());
        
        var key = args[0];
        var value = args[1];
        
        if(key is not BulkString keyString ||  value is not BulkString valueString)
            return new SimpleError("ERR Incorrect arguments"u8.ToArray());

        if (args.Length > 2) 
            return SetWithOptions(args[2..], keyString, valueString, storage);

        storage[keyString] = new RedisEntry(value, null);
        return new SimpleString("OK"u8.ToArray());

    }

    private static RespObject SetWithOptions(Span<RespObject> options,
        BulkString key, BulkString value,
        Dictionary<BulkString, RedisEntry> storage)
    {
        if (options.Length < 2)
            return new SimpleError("ERR Incorrect number of optional arguments"u8.ToArray());

        var optionObject = options[0];
        var timeObject = options[1];
        
        if (optionObject is not BulkString optionString || timeObject is not BulkString timeString)
            return new SimpleError("ERR Incorrect optional arguments"u8.ToArray());

        if (!int.TryParse(Encoding.UTF8.GetString(timeString.Value), out int expireTime))
            return new SimpleError("ERR Incorrect expire argument"u8.ToArray());

        switch (Encoding.UTF8.GetString(optionString.Value).ToUpper())
        {
            case "EX" : SetExpiry(expireTime * 1000, key, value, storage); break;
            case "PX" :  SetExpiry(expireTime, key, value, storage); break;
        }
        return new SimpleString("OK"u8.ToArray());
    }
    
    private static void SetExpiry(int time, 
        BulkString key, BulkString value, 
        Dictionary<BulkString, RedisEntry> storage)
    {
        storage[key] = new RedisEntry(value, DateTime.UtcNow.AddMilliseconds(time));
    }

}