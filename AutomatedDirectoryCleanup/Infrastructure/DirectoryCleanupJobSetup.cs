using Microsoft.Extensions.Options;
using Quartz;

namespace Infrastructure;

public sealed class DirectoryCleanupJobSetup : IConfigureOptions<QuartzOptions>
{
    void IConfigureOptions<QuartzOptions>.Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(DirectoryCleanupJob));
        options
            .AddJob<DirectoryCleanupJob>(job =>
            {
                job.WithIdentity(jobKey);
                job.WithDescription("Cleans out old files in a directory daily.");
            })
            .AddTrigger(trigger =>
                trigger
                    .ForJob(jobKey)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInHours(24)
                            .RepeatForever()));
    }
}
