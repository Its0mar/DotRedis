using System.Dynamic;
using System.Text;

public class RedisRequest
{
    public string Command {get; private set;} = string.Empty;
    public List<string> Arguments {get; private set;} = new();


    public RedisRequest(byte[] buffer, int bytesRead)
    {
        string rawData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Parse(rawData);
    }


    private void Parse(string data)
    {
        string[] parts = data.Split("\r\n");
        if (parts.Length > 2)
        {
            Command = parts[2].ToUpper();
        } 

        if (parts.Length > 4)
        {
            Arguments.Add(parts[4]);
        }
    }
}