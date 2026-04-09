using codecrafters_redis.Models;
using RedisEntry = codecrafters_redis.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class RespCommands
{
    public static SimpleString Ping() => Commands.Ping.Execute();
    public static RespObject Echo(Span<RespObject> args) => Commands.Echo.Execute(args);
    public static RespObject Set(Span<RespObject> args, Dictionary<BulkString, RedisEntry> variables)
        => Commands.Set.Execute(args, variables);
    public static RespObject Get(Span<RespObject> args, Dictionary<BulkString, RedisEntry> variables) 
        => Commands.Get.Execute(args, variables);
    public static RespObject RPush(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.RPush.Execute(args, arrays);
    public static RespObject LPush(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.LPush.Execute(args, arrays);
    public static RespObject LRange(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.LRange.Execute(args, arrays);
}