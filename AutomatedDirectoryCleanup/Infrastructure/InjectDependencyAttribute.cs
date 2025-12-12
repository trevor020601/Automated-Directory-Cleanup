using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

[AttributeUsage(AttributeTargets.Class)]
public class InjectDependencyAttribute(ServiceLifetime lifetime) : Attribute
{
    public ServiceLifetime Lifetime { get; } = lifetime;
}
