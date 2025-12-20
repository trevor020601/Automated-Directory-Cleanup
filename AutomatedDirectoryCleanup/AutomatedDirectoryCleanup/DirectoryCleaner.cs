using Microsoft.Extensions.Logging;
using Polly;
using System.Collections.Concurrent;

namespace AutomatedDirectoryCleanup;

public class DirectoryCleaner(ILogger<DirectoryCleaner> logger)
{
    private const int _errorCodeBits = 0x0000FFFF;
    // https://learn.microsoft.com/en-us/dotnet/standard/io/handling-io-errors#handling-ioexception
    private const int _errorCodeSharingViolation = 32;

    public void DeleteOldFilesByExtension(CleanupDirectory cleanupDirectory)
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

        var exceptions = new ConcurrentQueue<Exception>();
        Parallel.ForEach(filesToDelete, file =>
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
                exceptions.Enqueue(ex);
            }
        });

        if (!exceptions.IsEmpty)
        {
            throw new AggregateException(exceptions);
        }
    }
}
