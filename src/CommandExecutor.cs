using System.Text;
using codecrafters_redis.Commands;
using codecrafters_redis.Models;

namespace codecrafters_redis;

public class CommandExecutor
{
    private readonly RedisDatabase _database = new RedisDatabase();
    
    public RespObject Execute(RespList? list)
    {
        if (list == null || list.Items.Count == 0)
            return new SimpleError("ERR Null or empty command array"u8.ToArray());
        
        var allArgs = list.Items.ToArray().AsSpan();

        if (allArgs[0] is not BulkString bulkString)
            return new SimpleError("ERR Incorrect command array"u8.ToArray());

        var commandArgs = allArgs[1..];
        
        return Encoding.UTF8.GetString(bulkString.Value).ToUpper() switch
        {
            "PING" => RespCommands.Ping(),
            "ECHO" => RespCommands.Echo(commandArgs),
            "SET" => RespCommands.Set(commandArgs, _database.Storage),
            "GET" => RespCommands.Get(commandArgs, _database.Storage),
            "RPUSH" => RespCommands.RPush(commandArgs, _database.Storage),
            "LPUSH" => RespCommands.LPush(commandArgs, _database.Storage),
            "LRANGE" => RespCommands.LRange(commandArgs, _database.Storage),
            "LLEN" => RespCommands.LLen(commandArgs, _database.Storage),
            "LPOP" => RespCommands.LPop(commandArgs, _database.Storage),
            _ => new SimpleError("ERR Unknown command"u8.ToArray())
        };
    }
}