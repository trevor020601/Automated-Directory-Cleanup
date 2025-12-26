namespace AutomatedDirectoryCleanup;

public sealed class CleanupDirectory
{
    public DirectoryInfo Directory { get; private set; }

    public IEnumerable<string> Extensions { get; set; } = ["*"];

    public long TicksSinceCreation { get; set; }

    public TimeSpan TimeSpanSinceCreation => TimeSpan.FromTicks(TicksSinceCreation);

    public bool Exists => Directory.Exists;

    public CleanupDirectory(string directoryPath)
    {
        // This will only work with Windows and if long path support is disabled
        if (directoryPath.Length >= FileIOConstants.MAX_PATH_LENGTH)
        {
            throw new PathTooLongException($"Path provided is too long: {directoryPath}");
        }

        Directory = new DirectoryInfo(directoryPath);
    }
}
