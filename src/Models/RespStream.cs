namespace codecrafters_redis.Models;

public class RespStream : RespObject
{
    public List<StreamEntry> Entries { get; } = [];


    public override byte[] EncodeToBytes()
    {
        throw new NotImplementedException();
    }
}

public record StreamEntry(string Id, Dictionary<string, string> Values);