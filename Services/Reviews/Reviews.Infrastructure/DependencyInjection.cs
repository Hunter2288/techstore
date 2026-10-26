using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reviews.Application.Common.Interfaces;
using Reviews.Domain.Interfaces;
using Reviews.Infrastructure.Persistence;
using Reviews.Infrastructure.Seeding;

namespace Reviews.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        MongoMappings.Register();

        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        services.PostConfigure<MongoDbSettings>(settings =>
        {
            var connectionString =
                configuration.GetConnectionString("ReviewsDb");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                settings.ConnectionString = connectionString;
            }
        });

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<ReviewRepository>();
        services.AddScoped<ReviewQueries>();
        services.AddScoped<IReviewRepository>(sp => sp.GetRequiredService<ReviewRepository>());
        services.AddScoped<IReviewQueries>(sp => sp.GetRequiredService<ReviewQueries>());

        // Hosted services стартують у порядку реєстрації.
        services.AddHostedService<SchemaMigrationService>();
        services.AddHostedService<IndexCreationService>();
        services.AddHostedService<ReviewSeeder>();

        return services;
    }
}
