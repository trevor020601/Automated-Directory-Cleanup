namespace AutomatedDirectoryCleanup;

public sealed class CleanupDirectory
{
    public string DirectoryPath { get; set; } = default!;

    public List<string> Extensions { get; set; } = ["*"];

    public int Age { get; set; }
}
