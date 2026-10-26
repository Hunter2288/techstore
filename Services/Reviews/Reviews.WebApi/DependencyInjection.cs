using Microsoft.Extensions.DependencyInjection;

namespace Reviews.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(
        this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}