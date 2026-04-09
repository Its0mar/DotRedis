namespace codecrafters_redis;

public static class CommandParserUtils
{
    public static int FindNextTerminatorIndex(Span<byte> command, int startIndex)
    {
        while (startIndex < command.Length && command[startIndex] != '\r')
            startIndex++;
        if (startIndex == command.Length || command[startIndex + 1] != '\n')
            return -1;
        
        return startIndex;
    }
}