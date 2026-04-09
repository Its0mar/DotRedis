using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis.Models;

namespace codecrafters_redis;

public class Server
{
    private readonly CommandExecutor _commandExecutor = new();
    
    public async Task Run()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 6379);
        server.Start();
        Console.WriteLine("Server started");
        try
        {
            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Client connected");
                _ = Task.Run(() => ProcessClientAsync(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task ProcessClientAsync(TcpClient client)
    {
        var stream = client.GetStream(); 
        var buffer = new byte[4096];
        var command = new MemoryStream();
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                    break;
                Console.Write(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                await command.WriteAsync(buffer.AsMemory(0, bytesRead));
                if (CommandParser.TryParseCommand(command.GetBuffer(), out RespArray? array))
                {
                    var result = _commandExecutor.Execute(array);
                    await stream.WriteAsync(result.EncodeToBytes());
                    command.SetLength(0);
                }
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Failed to write to stream");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}