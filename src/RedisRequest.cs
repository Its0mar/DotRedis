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
        string[] parts = data.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0 || !parts[0].StartsWith('*')) return;
        if (!int.TryParse(parts[0].Substring(1), out int elementCount)) return;

        int currentIndex = 1;
        
        for (int i = 0; i < elementCount; i++)
        {
            if (currentIndex >= parts.Length) break;

            if (parts[currentIndex].StartsWith('$'))
            {
                string value = parts[currentIndex + 1];

                if (i == 0) Command = value.ToUpper();
                else Arguments.Add(value);
            }
            currentIndex += 2;
        }
    }
}