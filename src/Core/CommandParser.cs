using System.Text;
using codecrafters_redis.Models;

namespace codecrafters_redis.Core;

public static class CommandParser
{
    private const int TerminatorLength = 2;
    
    public static bool TryParseCommand(byte[] command, out RespList? result)
    {
        Span<byte> span = command.AsSpan();
        int elementStartIndex = 0;
        if (!TryParseArray(span, ref elementStartIndex, out RespList? array))
        {
            result = null;
            return false;
        }
        
        result = array;
        return true;
    }

    private static bool TryParseElement(Span<byte> element, ref int elementStartIndex, out RespObject? respObject)
    {
        var firstSymbol = (char)element[elementStartIndex];
        switch (firstSymbol)
        {
            case '+': return TryParseSimpleString(element, ref elementStartIndex, out respObject);
            case '-': return TryParseSimpleError(element, ref elementStartIndex, out respObject);
            case ':': return TryParseInteger(element, ref elementStartIndex, out respObject);
            case '$': return TryParseBulkString(element, ref elementStartIndex, out respObject);
            default:
            {
                respObject = null;
                return false;
            }
        }
    }

    private static bool TryParseSimpleString(Span<byte> element, ref int elementStartIndex,
        out RespObject? simpleString)
    {
        var nextTerminatorIndex = CommandParserUtils.FindNextTerminatorIndex(element, elementStartIndex);
        if (nextTerminatorIndex == -1)
        {
            simpleString = null;
            return false;
        }
        
        simpleString = new SimpleString(element[(elementStartIndex + 1)..nextTerminatorIndex].ToArray());
        elementStartIndex = nextTerminatorIndex + TerminatorLength;
        
        return true;
    }


    private static bool TryParseSimpleError(Span<byte> element, ref int elementStartIndex, out RespObject? simpleError)
    {
        var nextTerminatorIndex = CommandParserUtils.FindNextTerminatorIndex(element, elementStartIndex);
        if (nextTerminatorIndex == -1)
        {
            simpleError = null;
            return false;
        }
    
        simpleError = new SimpleError(element[(elementStartIndex + 1)..nextTerminatorIndex].ToArray());
        elementStartIndex = nextTerminatorIndex + TerminatorLength; 
        return true;
    }
    
    private static bool TryParseInteger(Span<byte> element, ref int elementStartIndex, out RespObject? integer)
    {
        var nextTerminatorIndex = CommandParserUtils.FindNextTerminatorIndex(element, elementStartIndex);
        if (nextTerminatorIndex == -1)
        {
            integer = null;
            return false;
        }
        var numberSpan = element.Slice(elementStartIndex + 1, nextTerminatorIndex - (elementStartIndex + 1));
        
        integer = new Integer(long.Parse(Encoding.UTF8.GetString(numberSpan)));
        elementStartIndex = nextTerminatorIndex + TerminatorLength;
        return true;
    }
    
    private static bool TryParseBulkString(Span<byte> element, ref int elementStartIndex, out RespObject bulkString)
    {
        var nextTerminatorIndex = CommandParserUtils.FindNextTerminatorIndex(element, elementStartIndex);
        var lengthSpan = element.Slice(elementStartIndex + 1, nextTerminatorIndex - (elementStartIndex + 1));
        long length = long.Parse(Encoding.UTF8.GetString(lengthSpan));
        
        int dataSpanStart = nextTerminatorIndex + TerminatorLength;
        nextTerminatorIndex = CommandParserUtils.FindNextTerminatorIndex(element, dataSpanStart);
        var dataSpan = element.Slice(dataSpanStart, nextTerminatorIndex - dataSpanStart);
        
        elementStartIndex = nextTerminatorIndex + TerminatorLength;
        bulkString = new BulkString(dataSpan.ToArray(), length);
        return true;
    }
    
    private static bool TryParseArray(Span<byte> element, ref int elementStartIndex, out RespList? result)
    {
        char firstSymbol = (char)element[elementStartIndex];
    
        if (firstSymbol != '*')
            throw new Exception("Expected array");

        var nextTerminatorIndex = CommandParserUtils.FindNextTerminatorIndex(element, elementStartIndex);
        int numOfElements = int.Parse(Encoding.UTF8.GetString(element.Slice(elementStartIndex + 1, nextTerminatorIndex - elementStartIndex - 1)));
        int nextElementStartIndex = nextTerminatorIndex + TerminatorLength;
    
        var list = new LinkedList<RespObject>();

        for (int i = 0; i < numOfElements; i++)
        {
            if (!TryParseElement(element, ref nextElementStartIndex, out RespObject? respObject))
            {
                result = null;
                return false;
            }
        
            if (respObject is not null) 
                list.AddLast(respObject);
        }
            
        result = new RespList(list);
        elementStartIndex = nextElementStartIndex; 
    
        return true;
    }
}