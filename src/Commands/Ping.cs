using codecrafters_redis.Models;

namespace codecrafters_redis.Commands;

public static class Ping
{
    public static SimpleString Execute()
    {
        return new SimpleString("PONG"u8.ToArray());
    }
}