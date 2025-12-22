using Microsoft.Extensions.Logging;
using Polly;
using System.Collections.Concurrent;

namespace AutomatedDirectoryCleanup;

public interface IDirectoryCleaner
{
    public int DeleteOldFilesByExtension(CleanupDirectory cleanupDirectory);
}

public class DirectoryCleaner(ILogger<DirectoryCleaner> logger) : IDirectoryCleaner
{
    private const int _errorCodeBits = 0x0000FFFF;
    // https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors#handling-ioexception
    private const int _errorCodeSharingViolation = 32;

    public int DeleteOldFilesByExtension(CleanupDirectory cleanupDirectory)
    {
        if (!cleanupDirectory.Exists)
        {
            throw new DirectoryNotFoundException(cleanupDirectory.Directory.FullName);
        }

        var filesToDelete = cleanupDirectory.Extensions
            .SelectMany(extension => cleanupDirectory.Directory.EnumerateFiles($"*.{extension}"))
            .Where(f => f.CreationTime < DateTime.Now.AddDays(-cleanupDirectory.TimeSpanSinceCreation.Days));

        var retryStrategyOptions = new Polly.Retry.RetryStrategyOptions()
        {
            ShouldHandle = new PredicateBuilder().Handle<IOException>(ex => (ex.HResult & _errorCodeBits) == _errorCodeSharingViolation),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Name = "Retry Strategy for Locked Files",
            Delay = TimeSpan.FromSeconds(5),
            OnRetry = args =>
            {
                logger.LogInformation("Retrying to delete file, Attempt: {0}", args.AttemptNumber);
                return default;
            }
        };

        var pipelineBuilder = new ResiliencePipelineBuilder().AddRetry(retryStrategyOptions).Build();

        var numberOfFilesDeleted = 0;
        var exceptions = new ConcurrentQueue<Exception>();
        Parallel.ForEach(
            filesToDelete,
            () => 0, // localInit: Function to initialize the local counter for each task (starts at 0)
            (file, _, localCount) =>
        {
            try
            {
                pipelineBuilder.Execute(() => {
                    file.Delete();
                    localCount++;
                    //Interlocked.Increment(ref numberOfFilesDeleted);
                    logger.LogInformation("{Name} has been deleted.", file.Name);
                });
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }

            return localCount;
        },
        localCount =>
        {
            Interlocked.Add(ref numberOfFilesDeleted, localCount);
        });

        if (!exceptions.IsEmpty)
        {
            foreach (var ex in exceptions)
            {
                logger.LogError(ex, "An exception has occurred during the clean up: {Message}", ex.Message);
            }
        }

        return numberOfFilesDeleted;
    }
}
