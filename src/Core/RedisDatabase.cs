using codecrafters_redis.Models;

namespace codecrafters_redis.Core;

public class RedisDatabase
{
    public record RedisEntry(RespObject Value, DateTime? Expiration);
    public readonly Dictionary<BulkString,  RedisEntry> Storage = new();
    
}