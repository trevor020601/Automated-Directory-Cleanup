using AutomatedDirectoryCleanup;
using Quartz;

namespace Infrastructure;

[DisallowConcurrentExecution]
public sealed class DirectoryCleanupJob(DirectoryCleaner directoryCleaner) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var cleanupDir = new CleanupDirectory(@"C:\testdir")
        {
            Age = 30,
            Extensions = ["*"]
        };
        directoryCleaner.DeleteOldFilesByExtension(cleanupDir);
        return Task.CompletedTask;
    }
}
