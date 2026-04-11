namespace codecrafters_redis.Models;

public class SimpleString(byte[] value) : RespObject
{
    public byte[] Value => value;

    public override byte[] EncodeToBytes()
    {
        return [.."+"u8.ToArray(), ..Value, .."\r\n"u8.ToArray()];
    }
}