using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Reflection;

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

    public static IServiceCollection AddServicesByAttribute(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var classes = assembly.GetTypes()
                                                    .Where(t => t.IsClass &&
                                                                    t.GetCustomAttribute<InjectDependencyAttribute>() != null);

            foreach (var cls in classes)
            {
                var attribute = cls.GetCustomAttribute<InjectDependencyAttribute>();

                var implementation = assembly.GetTypes()
                                                  .FirstOrDefault(t => t.IsClass &&
                                                                           !t.IsAbstract &&
                                                                           cls.IsAssignableFrom(t));

                if (implementation == null)
                {
                    continue;
                }

                // TODO: Maybe throw an exception if the attribute doesn't exist or figure out some other graceful way to handle this case...
                switch (attribute!.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(cls, implementation);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(cls, implementation);
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped(cls, implementation);
                        break;
                }
            }
        }

        return services;
    }
}
