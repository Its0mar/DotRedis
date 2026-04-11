namespace codecrafters_redis.Models;

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