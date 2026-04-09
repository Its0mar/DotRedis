namespace codecrafters_redis;

public class Program
{
    public static async Task Main()
    {
        var server = new Server();
        await server.Run();
    }
}