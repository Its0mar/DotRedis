using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");

ConcurrentDictionary<string, string> store = [];

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

                if (req.Command == "PING")
                {
                    client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
                }

                else if (req.Command == "ECHO")
                {
                    string echoVal = req.Arguments[0];
                    string response = $"${echoVal.Length}\r\n{echoVal}\r\n";
                    client.Send(Encoding.UTF8.GetBytes(response));
                }

                else if (req.Command == "SET")
                {
                    store[req.Arguments[0]] = req.Arguments[1];
                    string response = $"+OK\r\n";
                    client.Send(Encoding.UTF8.GetBytes(response));
                }

                else if (req.Command == "GET")
                {
                    string key = req.Arguments[0];

                    if (store.TryGetValue(key, out string? value))
                    {
                        string response = $"${value.Length}\r\n{value}\r\n";
                        client.Send(Encoding.UTF8.GetBytes(response));
                    }

                    else
                    {
                        client.Send(Encoding.UTF8.GetBytes("$-1\r\n"));
                    }
                }
            }
            
            catch (SocketException)
            {
                break;
            }
    
        }
    }
}


