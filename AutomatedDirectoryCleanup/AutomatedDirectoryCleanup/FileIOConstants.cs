namespace AutomatedDirectoryCleanup;

public static class FileIOConstants
{
    public const int MAX_PATH_LENGTH = 260; // Standard Windows limit
    public const int _errorCodeBits = 0x0000FFFF;
    // https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors#handling-ioexception
    public const int _errorCodeSharingViolationWindows = 32;
    public const int _errorChodeSharingViolationLinux = 11;
}
