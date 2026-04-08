using codecrafters_redis.Enums;

namespace codecrafters_redis.Models;

public struct StoredValue
{
    public DataType Type;
    public object Value;
    public DateTime? Expiry;

    public StoredValue Create(DataType dataType, object value, DateTime? expiry = null)
    {
        return new StoredValue
        {
            Type = dataType,
            Value = value,
            Expiry = expiry
        };
    }


}