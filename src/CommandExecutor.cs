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

        var commandArgs = allArgs;
        string command = Encoding.UTF8.GetString(bulkString.Value).ToUpper();
        
        Task<RespObject> task = command switch
        {
            "PING"  => Task.FromResult<RespObject>(Ping.Execute()),
            "GET"   => Task.FromResult(Get.Execute(commandArgs.AsSpan()[1..], _database.Storage)),
            "SET"   => Task.FromResult(Set.Execute(commandArgs.AsSpan()[1..], _database.Storage)),
    
            "RPUSH" => Task.FromResult(RPush.Execute(commandArgs.AsSpan()[1..], _database.Storage, _blockingManager)),
            "LPUSH" => Task.FromResult(LPush.Execute(commandArgs.AsSpan()[1..], _database.Storage)),

            "LPOP"  => Task.FromResult(LPop.Execute(commandArgs.AsSpan()[1..], _database.Storage)),

            "BLPOP" => BLPop.ExecuteAsync(commandArgs[1..], _database.Storage, _blockingManager),

            _ => Task.FromResult<RespObject>(new SimpleError("ERR Unknown command"u8.ToArray()))
        };

        return await task;
    }
}