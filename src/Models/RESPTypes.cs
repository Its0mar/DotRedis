using System.Text;

namespace codecrafters_redis.Models;

public abstract class RespObject
{
    public abstract byte[] EncodeToBytes();
}

public class SimpleString(byte[] value) : RespObject
{
    public byte[] Value => value;

    public override byte[] EncodeToBytes()
    {
        return [.."+"u8.ToArray(), ..Value, .."\r\n"u8.ToArray()];
    }
}

public class SimpleError(byte[] message) : RespObject
{
    public byte[] Message => message;

    public override byte[] EncodeToBytes()
    {
        return [.."-"u8.ToArray(), ..Message, .."\r\n"u8.ToArray()];
    }
}

public class Integer(long value) : RespObject
{
    public long Value => value;

    public override byte[] EncodeToBytes()
    {
        var v = Encoding.UTF8.GetBytes(Value.ToString());
        return [..":"u8.ToArray(), ..v, .."\r\n"u8.ToArray()];
    }
}

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

public class RespArray(RespObject[] objects) : RespObject
{
    public RespObject[] Objects => objects;
    
    public override byte[] EncodeToBytes()
    {
        var bytes = new MemoryStream();
        
        RespTypesUtility.WriteRespHeader(bytes, '*', Objects.Length);

        foreach (var respObj in  Objects)
        {
            bytes.Write(respObj.EncodeToBytes());            
        }
        
        return bytes.ToArray();
    }
}

public class RespList(LinkedList<RespObject> items) : RespObject
{
    public LinkedList<RespObject> Items => items;
    public bool IsNil { get; set; }
    
    public static RespList Nil { get; } = new RespList(new LinkedList<RespObject>()){IsNil = true};
    
    public override byte[] EncodeToBytes()
    {
        if (IsNil) return "*-1\r\n"u8.ToArray();
        
        var ms = new MemoryStream();
        
        RespTypesUtility.WriteRespHeader(ms, '*', Items.Count);
        
        foreach (var item in Items)
        {
            ms.Write(item.EncodeToBytes());
        }

        return ms.ToArray();
    }
}

public static class RespTypesUtility {
    public static void WriteRespHeader(MemoryStream ms, char prefix, int count)
    {
        ms.WriteByte((byte)prefix);
        ms.Write(Encoding.UTF8.GetBytes(count.ToString()));
        ms.Write("\r\n"u8);
    }
}