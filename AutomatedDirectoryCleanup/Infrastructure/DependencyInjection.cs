using AutomatedDirectoryCleanup;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void InjectQuartz(this IServiceCollection services)
    {
        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.AwaitApplicationStarted = true;
            options.WaitForJobsToComplete = true;
        });

        services.ConfigureOptions<DirectoryCleanupJobSetup>();
    }

    public static void AddAutomatedDirectoryCleanup(this IServiceCollection services)
    {
        services.AddTransient<IDirectoryCleaner, DirectoryCleaner>();
    }
}
