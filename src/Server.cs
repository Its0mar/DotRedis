using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");


TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();

while (true)
{
    Socket client = server.AcceptSocket(); // wait for client
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
                client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
            }
            catch (SocketException)
            {
                break;
            }
    
        }
    }
}


