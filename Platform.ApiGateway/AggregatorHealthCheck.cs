using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Platform.ApiGateway;

public sealed class AggregatorHealthCheck(IHttpClientFactory clients) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await clients.CreateClient("aggregator-health")
                .GetAsync("/health/ready", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("Aggregator and its dependencies are ready")
                : HealthCheckResult.Unhealthy($"Aggregator readiness returned {(int)response.StatusCode}");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Aggregator is unavailable", exception);
        }
    }
}
