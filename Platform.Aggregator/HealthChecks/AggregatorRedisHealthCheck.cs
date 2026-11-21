using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Platform.Aggregator.HealthChecks;

public sealed class AggregatorRedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    private readonly Microsoft.Extensions.DependencyInjection.RedisCacheHealthCheck _inner = new(cache);
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => _inner.CheckHealthAsync(context, cancellationToken);
}
