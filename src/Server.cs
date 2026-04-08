using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis.Protocol;
using codecrafters_redis.Services;

Console.WriteLine("Logs from your program will appear here!");

ConcurrentDictionaryStore concurrentDictionaryStore = new();


TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

while (true)
{
    Socket client = server.AcceptSocket(); 
    _ = Task.Run(() => HandleSocket(client));
}

void HandleSocket(Socket client)
{
    using (client)
    {
        while (true)
        {
            try
            {
                var buffer = new byte[1024];
                int bytesRead = client.Receive(buffer);

                if (bytesRead == 0) break;

                var req = new RedisRequest(buffer, bytesRead);
                var response = ProcessRequest(req, concurrentDictionaryStore);
               
                if (!string.IsNullOrEmpty(response))
                    client.Send(Encoding.UTF8.GetBytes(response));
            }

            catch (SocketException)
            {
                break;
            }

        }
    }
}

string ProcessRequest(RedisRequest req, ConcurrentDictionaryStore store)
{
    return req.Command switch
    {
        "PING" => "+PONG\r\n",
        
        "ECHO" => req.Arguments.Count > 0 
            ? $"${req.Arguments[0].Length}\r\n{req.Arguments[0]}\r\n" 
            : "-ERR missing argument\r\n",

        "SET" => HandleSet(req, store),

        "GET" => HandleGet(req, store),
        
        "RPUSH" => HandleRPush(req, store),
        
        "LRANGE" => HandleLRange(req, store),

        _ => "-ERR unknown command\r\n"
    };
}

string HandleSet(RedisRequest req, ConcurrentDictionaryStore store)
{
    if (req.Arguments.Count < 2) return "-ERR wrong number of arguments\r\n";
    
    var key = req.Arguments[0];
    var value = req.Arguments[1];
    DateTime? expiry = null;

    if (req.Arguments.Count >= 4)
    {
        string option = req.Arguments[2].ToUpper();
        if (long.TryParse(req.Arguments[3], out long time))
        {
            if (option == "PX") expiry = DateTime.UtcNow.AddMilliseconds(time);
            else if (option == "EX") expiry = DateTime.UtcNow.AddSeconds(time);
        }
    }
    store.Set(key, value, expiry);
    
    return $"+OK\r\n";
}

string HandleGet(RedisRequest req, ConcurrentDictionaryStore store)
{
    if (req.Arguments.Count == 0) return "-ERR missing key\r\n";
    
    var key = req.Arguments[0];
    var value = store.GetString(key);

    return value == null ? "$-1\r\n" : $"${value.Length}\r\n{value}\r\n";
}

string HandleRPush(RedisRequest req, ConcurrentDictionaryStore store)
{
    if (req.Arguments.Count < 2) return "-ERR wrong number of arguments\r\n";

    var key = req.Arguments[0];
    var values = req.Arguments.Skip(1).ToList();
    var count = store.AddToList(key, values);

    return $":{count}\r\n";
}

string HandleLRange(RedisRequest req, ConcurrentDictionaryStore store)
{
    if (req.Arguments.Count < 3) return "-ERR wrong number of arguments\r\n";

    var key = req.Arguments[0];
    if (!int.TryParse(req.Arguments[1], out var firstIndex) ||
            !int.TryParse(req.Arguments[2], out var lastIndex))
        return "-ERR value is not an integer or out of range\\r\\n\r\n";

    var values = store.GetList(key);
    if (values is null) return "*0\r\n";

    values = values.Skip(firstIndex).Take(lastIndex - firstIndex + 1).ToList();
    
    var sb = new StringBuilder();
    sb.Append($"*{values.Count}\r\n");

    foreach (var v in values)
    {
        sb.Append($"${v.Length}\r\n{v}\r\n");
    }

    return sb.ToString();
}
