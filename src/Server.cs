using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");


TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

while (true)
{
    Console.WriteLine("--> Waiting for a NEW client..."); 
    Socket client = server.AcceptSocket(); 
    Console.WriteLine("--> Client connected! Starting Task...");
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
                // Console.WriteLine($"[DEBUG] Raw bytes received");
                var req = new RedisRequest(buffer, bytesRead);
                if (req.Command.ToUpper() == "PING")
                {
                    client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
                }
                else if (req.Command.ToUpper() == "ECHO")
                {
                    string echoVal = req.Arguments[0];
                    string response = $"${echoVal.Length}\r\n{echoVal}\r\n";
                    client.Send(Encoding.UTF8.GetBytes(response));
                }
                // Console.WriteLine($"command is {req.Command}");
            }
            catch (SocketException)
            {
                break;
            }
    
        }
    }
}


