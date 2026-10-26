using Microsoft.Extensions.DependencyInjection;

namespace Reviews.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}