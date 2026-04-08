using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_redis.Protocol;
using codecrafters_redis.Services;

Console.WriteLine("Logs from your program will appear here!");

ConcurrentDictionaryStore concurrentDictionaryStore = new();
ListStore listStore = new();

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
                var response = ProcessRequest(req, concurrentDictionaryStore, listStore);
               
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

string ProcessRequest(RedisRequest req, ConcurrentDictionaryStore store, ListStore ltStore)
{
    return req.Command switch
    {
        "PING" => "+PONG\r\n",
        
        "ECHO" => req.Arguments.Count > 0 
            ? $"${req.Arguments[0].Length}\r\n{req.Arguments[0]}\r\n" 
            : "-ERR missing argument\r\n",

        "SET" => HandleSet(req, store),

        "GET" => HandleGet(req, store),
        
        "RPUSH" => HandleRpush(req, ltStore),

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

string HandleRpush(RedisRequest req, ListStore store)
{
    if (req.Arguments.Count < 2) return "-ERR wrong number of arguments\r\n";

    var key = req.Arguments[0];
    var value = req.Arguments[1];

    int count = store.AddToList(key, value);

    return $":{count}\r\n";

}
