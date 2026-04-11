using System.Collections.Concurrent;
using codecrafters_redis.Models;

namespace codecrafters_redis.Services;

public class BlockingManager
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<TaskCompletionSource<RespObject>>> _waiters = new();

    public Task<RespObject> WaitForKey(string key)
    {
        var tcs = new TaskCompletionSource<RespObject>(TaskCreationOptions.RunContinuationsAsynchronously);

        var queue = _waiters.GetOrAdd(key, _ => new ConcurrentQueue<TaskCompletionSource<RespObject>>());
        queue.Enqueue(tcs);
        
        return tcs.Task;
    }
    
    public void CancelWaiter(string key, Task<RespObject> waiterTask)
    {
        if (_waiters.TryGetValue(key, out var queue))
        {
            var remainingWaiters = queue.Where(t => t.Task != waiterTask).ToList();
            
            _waiters[key] = new ConcurrentQueue<TaskCompletionSource<RespObject>>(remainingWaiters);
        }
    }

    public bool ResolveWaiter(string key, RespObject value)
    {
        if (_waiters.TryGetValue(key, out var queue))
        {
            while (queue.TryDequeue(out var tcs))
            {
                if (tcs.TrySetResult(value))
                {
                    return true;
                }
            }
        }
        return false;
    }
}