namespace codecrafters_redis.Models;

public class SimpleError(byte[] message) : RespObject
{
    public byte[] Message => message;

    public override byte[] EncodeToBytes()
    {
        return [.."-"u8.ToArray(), ..Message, .."\r\n"u8.ToArray()];
    }
}