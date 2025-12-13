using Microsoft.Extensions.Logging;
using Polly;

namespace AutomatedDirectoryCleanup;

public class DirectoryCleaner(ILogger<DirectoryCleaner> logger)
{
    private const int _errorCodeBits = 0x0000FFFF;
    // https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors#handling-ioexception
    private const int _errorCodeSharingViolation = 32;

    public void DeleteOldFilesByExtensionPolly(CleanupDirectory cleanupDirectory)
    {
        if (!Directory.Exists(cleanupDirectory.DirectoryPath))
        {
            throw new DirectoryNotFoundException(cleanupDirectory.DirectoryPath);
        }

        var files = new FileSystem().GetFiles(cleanupDirectory.DirectoryPath);
        var filesToDelete = files.Where(f => cleanupDirectory.Extensions.Contains(f.Extension[1..]))
            .Where(f => f.CreationTime < DateTime.Now.AddDays(-cleanupDirectory.AgeTimeSpan.Days))
            .ToList();

        var retryStrategyOptions = new Polly.Retry.RetryStrategyOptions()
        {
            ShouldHandle = new PredicateBuilder().Handle<IOException>(ex => (ex.HResult & _errorCodeBits) == _errorCodeSharingViolation),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(5),
            OnRetry = args =>
            {
                logger.LogInformation("Retrying to delete file, Attempt: {0}", args.AttemptNumber);
                return default;
            }
        };

        var pipelineBuilder = new ResiliencePipelineBuilder().AddRetry(retryStrategyOptions).Build();

        foreach (var file in filesToDelete)
        {
            try
            {
                pipelineBuilder.Execute(() => {
                    file.Delete();
                    logger.LogInformation("{Name} has been deleted.", file.Name);
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Name} has been skipped due to exception: {Message}", file.Name, ex.Message);
                continue;
            }
        }
    }

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
                    file.Delete();
                    logger.LogInformation("{Name} has been deleted.", file.Name);
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
