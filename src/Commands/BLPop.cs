using System.Text;
using codecrafters_redis.Models;
using codecrafters_redis.Services;

namespace codecrafters_redis.Commands;

public static class BLPop
{
    public static async Task<RespObject> ExecuteAsync(RespObject[] args, Dictionary<BulkString, RedisDatabase.RedisEntry> storage, BlockingManager blockingManager)
    {
        if (args.Length < 2)
            return new SimpleError("ERR wrong number of arguments for 'blpop' command"u8.ToArray());
        
        var keyBs = (BulkString)args[0];
        var timeoutBs = (BulkString)args[^1];
        double.TryParse(Encoding.UTF8.GetString(timeoutBs.Value), out double timeoutSeconds);

        if (storage.TryGetValue(keyBs, out var entry) && entry.Value is RespList { Items.Count: > 0 } respList)
        {
            var item = respList.Items.First!.Value;
            respList.Items.RemoveFirst();
            if (respList.Items.Count == 0) storage.Remove(keyBs);
            
            return CreateBlpopResponse(keyBs, item);
        }

        var waiterTask = blockingManager.WaitForKey(keyBs.ToString());

        if (timeoutSeconds > 0)
        {
            var delayTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
            var completedTask = await Task.WhenAny(waiterTask, delayTask);
            if (completedTask == delayTask)
            {
                // Timeout reached
                return RespArray.Null; 
            }
        }
        var poppedItem = await waiterTask;
        return CreateBlpopResponse(keyBs, poppedItem);
    }
    
    
    private static RespArray CreateBlpopResponse(BulkString key, RespObject value)
    {
        return new RespArray([key, value]);
    }
}