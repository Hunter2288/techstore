using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Reviews.WebApi.HealthChecks;

public sealed class ReviewsRedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    private readonly Microsoft.Extensions.DependencyInjection.RedisCacheHealthCheck _inner = new(cache);
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => _inner.CheckHealthAsync(context, cancellationToken);
}
