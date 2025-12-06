namespace AutomatedDirectoryCleanup;

public static class FileInfoExtensions
{
    private const int _errorCodeBits = 0x0000FFFF;
    // https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors#handling-ioexception
    private const int _errorCodeSharingViolation = 32;

    public static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read);
            return false;
        }
        // Check HResult property and its lower 16 bits 
        catch (IOException ex) when ((ex.HResult & _errorCodeBits) == _errorCodeSharingViolation)
        {
            // TODO: Log that file is locked
            return true;
        }
    }

    public static bool IsFileLockedGeneric(FileInfo file)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read);
            return false;
        }
        catch (IOException)
        {
            // TODO: Log that file is locked
            return true;
        }
    }
}
