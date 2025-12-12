//using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutomatedDirectoryCleanup;

//[InjectDependency(ServiceLifetime.Transient)]
public class DirectoryCleaner(ILogger<DirectoryCleaner> logger)
{
    public void DeleteOldFilesByExtension(CleanupDirectory cleanupDirectory)
    {
        if (!Directory.Exists(cleanupDirectory.DirectoryPath))
        {
            throw new DirectoryNotFoundException(cleanupDirectory.DirectoryPath);
        }

        var files = new FileSystem().GetFiles(cleanupDirectory.DirectoryPath);
        var filesToDelete = files.Where(f => cleanupDirectory.Extensions.Contains(f.Extension[1..]))
            .Where(f => f.CreationTime < DateTime.Now.AddDays(-cleanupDirectory.AgeTimeSpan.Days))
            .ToList();

        foreach (var file in filesToDelete)
        {
            try
            {
                var isLocked = FileInfoExtensions.IsFileLocked(file);
                if (!isLocked)
                {
                    logger.LogInformation("{Name} has been deleted.", file.Name);
                    file.Delete();
                }
                else
                {
                    logger.LogInformation("{Name} is locked. Skipping...", file.Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Name} has been skipped due to exception: {Message}", file.Name, ex.Message);
                continue;
            }
        }
    }
}
