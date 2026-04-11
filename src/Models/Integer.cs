using System.Text;

namespace codecrafters_redis.Models;

public class Integer(long value) : RespObject
{
    public long Value => value;

    public override byte[] EncodeToBytes()
    {
        var v = Encoding.UTF8.GetBytes(Value.ToString());
        return [..":"u8.ToArray(), ..v, .."\r\n"u8.ToArray()];
    }
}