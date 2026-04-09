using codecrafters_redis.Models;

namespace codecrafters_redis.Commands;

public static class Echo
{
    public static RespObject Execute(Span<RespObject> args)
    {
        if (args.Length == 0 || args[0] is not BulkString bulkString)
            return new SimpleError("ERP Incorrect arguments"u8.ToArray());

        return bulkString;
    }
}