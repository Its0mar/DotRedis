using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Logs from your program will appear here!");


TcpListener server = new TcpListener(IPAddress.Any, 6379);
server.Start();
Socket client = server.AcceptSocket(); // wait for client

while (true)
{
    var buffer = new byte[1024];
    int bytesRead = client.Receive(buffer);
    if (bytesRead == 0) break;
    client.Send(Encoding.UTF8.GetBytes("+PONG\r\n"));
    
}

