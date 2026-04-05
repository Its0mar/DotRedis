using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");

ConcurrentDictionary<string, StoredValue> store = [];

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
                    var storedValue = new StoredValue {Value = req.Arguments[1]};

                    if (req.Arguments.Count > 2)
                    {
                        long.TryParse(req.Arguments[3], out var expiryLength);
                        if (req.Arguments[2].ToUpper() == "PX")
                        {
                            var expiry = DateTime.UtcNow.AddMilliseconds(expiryLength);
                            storedValue.Expiry = expiry;
                        }

                        else if (req.Arguments[2].ToUpper() == "EX")
                        {
                            var expiry = DateTime.UtcNow.AddSeconds(expiryLength);
                            storedValue.Expiry = expiry;
                        }    
                    }

                    store[req.Arguments[0]] = storedValue;
                    string response = $"+OK\r\n";
                    client.Send(Encoding.UTF8.GetBytes(response));
                }

                else if (req.Command == "GET")
                {
                    string key = req.Arguments[0];

                    if (store.TryGetValue(key, out StoredValue value))
                    {
                        if (value.Expiry is DateTime expiry && DateTime.UtcNow >= expiry)
                        {
                            store.TryRemove(key, out value);
                            client.Send(Encoding.UTF8.GetBytes("$-1\r\n"));
                        } 
                        else
                        {
                            string response = $"${value.Value.Length}\r\n{value.Value}\r\n";
                            client.Send(Encoding.UTF8.GetBytes(response));
                        }
                        
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


