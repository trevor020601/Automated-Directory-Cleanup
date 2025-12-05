namespace AutomatedDirectoryCleanup;

public class DirectoryCleaner
{
    public void DeleteOldFilesByExtension(CleanupDirectory cleanupDirectory)
    {
        if (cleanupDirectory.DirectoryPath is null)
        {
            throw new ArgumentNullException(cleanupDirectory.DirectoryPath);
        }

        var files = new FileSystem().GetFiles(cleanupDirectory.DirectoryPath);
        if (files.Length < 0)
        {
            return;
        }

        var filesToDelete = files.Where(f => cleanupDirectory.Extensions.Contains(f.Extension[1..]))
            .Where(f => f.CreationTime < DateTime.Now.AddDays(-cleanupDirectory.Age))
            .ToList();

        filesToDelete.ForEach(f => f.Delete());
    }
}
