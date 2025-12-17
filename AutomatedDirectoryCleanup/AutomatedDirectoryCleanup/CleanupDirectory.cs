namespace AutomatedDirectoryCleanup;

public sealed class CleanupDirectory
{
    const int MAX_PATH_LENGTH = 260; // Standard Windows limit

    public Uri DirectoryUri { get; set; } = default!;

    public List<string> Extensions { get; set; } = ["*"];

    public int Age { get; set; }

    public TimeSpan AgeTimeSpan => TimeSpan.FromDays(Age);

    public CleanupDirectory(string directoryPath)
    {
        // This will only work with Windows and if long path support is disabled
        if (directoryPath.Length >= MAX_PATH_LENGTH)
        {
            throw new PathTooLongException($"Path provided is too long: {directoryPath}");
        }

        DirectoryUri = new Uri(directoryPath);
        if (!DirectoryUri.IsUnc)
        {
            throw new UriFormatException("Path provided is not in UNC.");
        }
    }
}
