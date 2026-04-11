namespace codecrafters_redis.Models;

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