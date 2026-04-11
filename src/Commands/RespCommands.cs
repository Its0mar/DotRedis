using codecrafters_redis.Models;
using codecrafters_redis.Services;
using RedisEntry = codecrafters_redis.Core.RedisDatabase.RedisEntry;

namespace codecrafters_redis.Commands;

public static class RespCommands
{
    public static SimpleString Ping() => Commands.Ping.Execute();
    public static RespObject Echo(Span<RespObject> args) => Commands.Echo.Execute(args);
    public static RespObject Set(Span<RespObject> args, Dictionary<BulkString, RedisEntry> variables)
        => Commands.Set.Execute(args, variables);
    public static RespObject Get(Span<RespObject> args, Dictionary<BulkString, RedisEntry> variables) 
        => Commands.Get.Execute(args, variables);
    public static RespObject RPush(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays, BlockingManager  blockingManager)
        => Commands.RPush.Execute(args, arrays, blockingManager);
    public static RespObject LPush(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.LPush.Execute(args, arrays);
    public static RespObject LRange(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.LRange.Execute(args, arrays);
    public static RespObject LLen(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.LLen.Execute(args, arrays);
    public static RespObject LPop(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Commands.LPop.Execute(args, arrays);
    public static RespObject Type(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Types.Type.Execute(args, arrays);
    public static RespObject XAdd(Span<RespObject> args, Dictionary<BulkString, RedisEntry> arrays)
        => Stream.XAdd.Execute(args, arrays);
    
    
}