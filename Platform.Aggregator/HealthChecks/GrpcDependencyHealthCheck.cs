using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Platform.Aggregator.HealthChecks;

public abstract class GrpcDependencyHealthCheck(Grpc.Net.ClientFactory.GrpcClientFactory factory,
    ILogger<GrpcDependencyHealthCheck> logger, string dependency, string clientName) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await factory.CreateClient<Health.HealthClient>(clientName).CheckAsync(new HealthCheckRequest(),
                deadline: DateTime.UtcNow.AddSeconds(2), cancellationToken: cancellationToken);
            return response.Status == HealthCheckResponse.Types.ServingStatus.Serving
                ? HealthCheckResult.Healthy($"{dependency} is serving")
                : HealthCheckResult.Unhealthy($"{dependency} status: {response.Status}");
        }
        catch (RpcException exception)
        {
            logger.LogWarning(exception, "gRPC health check failed for {Dependency}", dependency);
            return HealthCheckResult.Unhealthy($"{dependency} is unavailable", exception);
        }
    }
}

public sealed class OrdersGrpcHealthCheck(Grpc.Net.ClientFactory.GrpcClientFactory factory,
    ILogger<GrpcDependencyHealthCheck> logger) : GrpcDependencyHealthCheck(factory, logger, "orders-api", "orders-health");
public sealed class CatalogGrpcHealthCheck(Grpc.Net.ClientFactory.GrpcClientFactory factory,
    ILogger<GrpcDependencyHealthCheck> logger) : GrpcDependencyHealthCheck(factory, logger, "catalog-api", "catalog-health");
public sealed class ReviewsGrpcHealthCheck(Grpc.Net.ClientFactory.GrpcClientFactory factory,
    ILogger<GrpcDependencyHealthCheck> logger) : GrpcDependencyHealthCheck(factory, logger, "reviews-api", "reviews-health");
