using System.Text;
using codecrafters_redis.Commands;
using codecrafters_redis.Models;
using codecrafters_redis.Services;

namespace codecrafters_redis;

public class CommandExecutor
{
    private readonly RedisDatabase _database = new();
    private readonly BlockingManager _blockingManager = new();
    
    public async Task<RespObject> ExecuteAsync(RespList? list)
    {
        if (list == null || list.Items.Count == 0)
            return new SimpleError("ERR Null or empty command array"u8.ToArray());
        
        RespObject[] allArgs = list.Items.ToArray();

        if (allArgs[0] is not BulkString bulkString)
            return new SimpleError("ERR Incorrect command array"u8.ToArray());

        var args = allArgs.AsSpan();
        string command = Encoding.UTF8.GetString(bulkString.Value).ToUpper();
        
        Task<RespObject> task = command switch
        {
            // Use RespCommands proxy methods
            "PING"   => Task.FromResult<RespObject>(RespCommands.Ping()),
            "ECHO"   => Task.FromResult(RespCommands.Echo(args[1..])),
            "GET"    => Task.FromResult(RespCommands.Get(args[1..], _database.Storage)),
            "SET"    => Task.FromResult(RespCommands.Set(args[1..], _database.Storage)),
            
            "RPUSH"  => Task.FromResult(RespCommands.RPush(args[1..], _database.Storage, _blockingManager)),
            "LPUSH"  => Task.FromResult(RespCommands.LPush(args[1..], _database.Storage)),
            
            "LRANGE" => Task.FromResult(RespCommands.LRange(args[1..], _database.Storage)),
            "LLEN"   => Task.FromResult(RespCommands.LLen(args[1..], _database.Storage)),
            "LPOP"   => Task.FromResult(RespCommands.LPop(args[1..], _database.Storage)),

            "BLPOP"  => BLPop.ExecuteAsync(allArgs[1..], _database.Storage, _blockingManager),

            _ => Task.FromResult<RespObject>(new SimpleError("ERR Unknown command"u8.ToArray()))
        };

        return await task;
    }
}