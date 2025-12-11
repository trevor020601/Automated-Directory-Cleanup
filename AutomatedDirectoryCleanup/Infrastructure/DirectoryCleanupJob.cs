using AutomatedDirectoryCleanup;
using Quartz;

namespace Infrastructure;

[DisallowConcurrentExecution]
public sealed class DirectoryCleanupJob(DirectoryCleaner directoryCleaner) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var cleanupDir = new CleanupDirectory
        {
            Age = 30,
            DirectoryPath = @"C:\testdir",
            Extensions = ["*"]
        };
        directoryCleaner.DeleteOldFilesByExtension(cleanupDir);
        return Task.CompletedTask;
    }
}
