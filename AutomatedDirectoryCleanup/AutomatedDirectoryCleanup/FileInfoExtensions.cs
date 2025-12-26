namespace AutomatedDirectoryCleanup;

public static class FileInfoExtensions
{
    public static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using var stream = file.Open(FileMode.Open, FileAccess.Read);
            return false;
        }
        // Check HResult property and its lower 16 bits 
        catch (IOException ex) when ((ex.HResult & FileIOConstants._errorCodeBits) == (
            OperatingSystem.IsWindows() ?
                FileIOConstants._errorCodeSharingViolationWindows :
                FileIOConstants._errorChodeSharingViolationLinux))
        {
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
            return true;
        }
    }
}
