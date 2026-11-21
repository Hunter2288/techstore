using System.Text.Json.Serialization;
using Catalog.Grpc;
using Orders.Grpc;
using Reviews.Grpc;
using Platform.Aggregator.Services;
using Platform.Aggregator.HealthChecks;
using Grpc.Net.Client.Configuration;
using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.CircuitBreaker;
using Grpc.Core;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddRedisDistributedCache("redis");
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddGrpcClient<OrdersService.OrdersServiceClient>(options =>
    options.Address = new Uri("https+http://orders-api"))
    .ConfigureChannel(channel => channel.ServiceConfig = RetryConfig(maxAttempts: 5));
builder.Services.AddGrpcClient<CatalogService.CatalogServiceClient>(options =>
    options.Address = new Uri("https+http://catalog-api"))
    .ConfigureChannel(channel => channel.ServiceConfig = RetryConfig(maxAttempts: 5));
builder.Services.AddGrpcClient<ReviewsService.ReviewsServiceClient>(options =>
    options.Address = new Uri("https+http://reviews-api"))
    .ConfigureChannel(channel => channel.ServiceConfig = RetryConfig(maxAttempts: 2));
builder.Services.AddGrpcClient<Health.HealthClient>("orders-health", options =>
    options.Address = new Uri("https+http://orders-api"));
builder.Services.AddGrpcClient<Health.HealthClient>("catalog-health", options =>
    options.Address = new Uri("https+http://catalog-api"));
builder.Services.AddGrpcClient<Health.HealthClient>("reviews-health", options =>
    options.Address = new Uri("https+http://reviews-api"));
builder.Services.AddScoped<FullOrderService>();
builder.Services.AddSingleton<AggregatorMetrics>();
builder.Services.AddResiliencePipeline("catalog-api", (pipeline, context) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<FullOrderService>>();
    var metrics = context.ServiceProvider.GetRequiredService<AggregatorMetrics>();
    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        MinimumThroughput = 4,
        SamplingDuration = TimeSpan.FromSeconds(30),
        BreakDuration = TimeSpan.FromSeconds(15),
        ShouldHandle = new PredicateBuilder().Handle<RpcException>(exception =>
            exception.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded),
        OnOpened = args =>
        {
            logger.LogError("Circuit opened for catalog-api for {Duration}", args.BreakDuration);
            metrics.CircuitEvent("catalog-api", "OnCircuitOpened");
            return ValueTask.CompletedTask;
        },
        OnClosed = _ =>
        {
            logger.LogInformation("Circuit closed for catalog-api");
            metrics.CircuitEvent("catalog-api", "OnCircuitClosed");
            return ValueTask.CompletedTask;
        },
        OnHalfOpened = _ =>
        {
            logger.LogInformation("Circuit half-open for catalog-api; allowing a probe");
            metrics.CircuitEvent("catalog-api", "OnCircuitHalfOpened");
            return ValueTask.CompletedTask;
        }
    });
});
builder.Services.AddResiliencePipeline("reviews-api", (pipeline, context) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<FullOrderService>>();
    var metrics = context.ServiceProvider.GetRequiredService<AggregatorMetrics>();
    pipeline.AddCircuitBreaker(new CircuitBreakerStrategyOptions
    {
        FailureRatio = 0.7,
        MinimumThroughput = 8,
        SamplingDuration = TimeSpan.FromSeconds(15),
        BreakDuration = TimeSpan.FromSeconds(5),
        ShouldHandle = new PredicateBuilder().Handle<RpcException>(exception =>
            exception.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded),
        OnOpened = args =>
        {
            logger.LogWarning("Circuit opened for reviews-api for {Duration}", args.BreakDuration);
            metrics.CircuitEvent("reviews-api", "OnCircuitOpened");
            return ValueTask.CompletedTask;
        },
        OnClosed = _ =>
        {
            logger.LogInformation("Circuit closed for reviews-api");
            metrics.CircuitEvent("reviews-api", "OnCircuitClosed");
            return ValueTask.CompletedTask;
        },
        OnHalfOpened = _ =>
        {
            logger.LogInformation("Circuit half-open for reviews-api; allowing a probe");
            metrics.CircuitEvent("reviews-api", "OnCircuitHalfOpened");
            return ValueTask.CompletedTask;
        }
    });
});
builder.Services.AddHealthChecks()
    .AddCheck<AggregatorRedisHealthCheck>("aggregator-redis", tags: ["cache", "redis", "ready"], timeout: TimeSpan.FromSeconds(3))
    .AddCheck<OrdersGrpcHealthCheck>("orders-grpc", tags: ["grpc", "downstream", "ready"], timeout: TimeSpan.FromSeconds(3))
    .AddCheck<CatalogGrpcHealthCheck>("catalog-grpc", tags: ["grpc", "downstream", "ready"], timeout: TimeSpan.FromSeconds(3))
    .AddCheck<ReviewsGrpcHealthCheck>("reviews-grpc", tags: ["grpc", "downstream", "ready"], timeout: TimeSpan.FromSeconds(3));

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapGet("/api/aggregator/orders/{id:long}/full", async (
    long id, FullOrderService service, AggregatorMetrics metrics, CancellationToken cancellationToken) =>
{
    var timer = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        var result = await service.GetFullOrderAsync(id, cancellationToken);
        metrics.Record(result is not null, result?.Warnings.Count > 0, timer.Elapsed.TotalMilliseconds);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }
    catch
    {
        metrics.Record(false, false, timer.Elapsed.TotalMilliseconds);
        throw;
    }
});
app.MapDefaultEndpoints();
app.Run();

static ServiceConfig RetryConfig(int maxAttempts) => new()
{
    MethodConfigs =
    {
        new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = maxAttempts,
                InitialBackoff = TimeSpan.FromMilliseconds(250),
                MaxBackoff = TimeSpan.FromSeconds(2),
                BackoffMultiplier = 2,
                RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable, Grpc.Core.StatusCode.ResourceExhausted }
            }
        }
    }
};
