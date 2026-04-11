using System.Text;

namespace codecrafters_redis.Models;

public static class RespTypesUtility {
    public static void WriteRespHeader(MemoryStream ms, char prefix, int count)
    {
        ms.WriteByte((byte)prefix);
        ms.Write(Encoding.UTF8.GetBytes(count.ToString()));
        ms.Write("\r\n"u8);
    }
}