using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
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
                var databaseName = MongoUrl.Create(connectionString).DatabaseName;
                if (!string.IsNullOrWhiteSpace(databaseName))
                {
                    settings.DatabaseName = databaseName;
                }
            }
        });

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            clientSettings.ClusterConfigurator = cluster =>
                cluster.Subscribe(new DiagnosticsActivityEventSubscriber());
            return new MongoClient(clientSettings);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<ReviewRepository>();
        services.AddScoped<ReviewQueries>();
        services.AddScoped<IReviewRepository>(sp => sp.GetRequiredService<ReviewRepository>());
        services.AddScoped<IReviewQueries>(sp => sp.GetRequiredService<ReviewQueries>());

        services.AddHostedService<SchemaMigrationService>();
        services.AddHostedService<IndexCreationService>();
        services.AddHostedService<ReviewSeeder>();

        return services;
    }
}
