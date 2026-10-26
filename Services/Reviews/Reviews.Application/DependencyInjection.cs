using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Reviews.Application.Common.Behaviors;

namespace Reviews.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, string? licenseKey = null)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = licenseKey;
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));      
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));   
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));  
        });

        services.AddAutoMapper(cfg => cfg.LicenseKey = licenseKey, assembly);
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}