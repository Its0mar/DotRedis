
using System.Text;

namespace codecrafters_redis.Models;

public class BulkString(byte[] value, long length) : RespObject
{
    public byte[] Value { get; } = value;
    public long Length { get; } = length;

    public static BulkString Null => new BulkString([], -1);
    
    public override byte[] EncodeToBytes()
    {
        var len = Encoding.UTF8.GetBytes(Length.ToString());
        if (Length is -1)
            return [.."$"u8.ToArray(), ..len, .."\r\n"u8.ToArray()];

        return [.."$"u8.ToArray(), ..len, .."\r\n"u8.ToArray(), ..Value, .."\r\n"u8.ToArray()];
    }
    
    public override bool Equals(object? obj)
    {
        return obj is BulkString b && Value.SequenceEqual(b.Value);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var b in Value)
                hash = hash * 31 + b;
            return hash;
        }
    }
    public override string ToString() => Encoding.UTF8.GetString(Value);
}