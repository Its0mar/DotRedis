using System.Text;
using codecrafters_redis.Commands;
using codecrafters_redis.Models;

namespace codecrafters_redis;

public class CommandExecutor
{
    private readonly RedisDatabase _database = new RedisDatabase();
    
    public RespObject Execute(RESPArray? array)
    {
        if (array == null || array.Objects.Length == 0)
            return new SimpleError("ERR Null or empty command array"u8.ToArray());

        if (array.Objects[0] is not BulkString bulkString)
            return new SimpleError("ERR Incorrect command array"u8.ToArray());

        var commandArgs = array.Objects.AsSpan()[1..];
        return Encoding.UTF8.GetString(bulkString.Value).ToUpper() switch
        {
            "PING" => RespCommands.Ping(),
            "ECHO" => RespCommands.Echo(commandArgs),
            "SET" => RespCommands.Set(commandArgs, _database.Storage),
            "GET" => RespCommands.Get(commandArgs, _database.Storage),
            "RPUSH" => RespCommands.RPush(commandArgs, _database.Storage),
            "LRANGE" => RespCommands.LRange(commandArgs, _database.Storage),
            _ => new SimpleError("ERR Unknown command"u8.ToArray())
        };
    }
}