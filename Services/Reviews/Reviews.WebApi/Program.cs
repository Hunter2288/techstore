using Reviews.Application;
using Reviews.Infrastructure;
using Reviews.WebApi;
using Reviews.WebApi.Middleware;
using Reviews.WebApi.HealthChecks;
using Reviews.WebApi.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Platform.EventBus;
using Reviews.Infrastructure.Persistence;
using Reviews.WebApi.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(
    endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddRedisDistributedCache("redis");
builder.AddRabbitMQClient("messaging");
builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);
builder.Services.AddSingleton<ReviewMetrics>();
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("reviews-db", tags: ["database", "mongodb", "ready"], timeout: TimeSpan.FromSeconds(5))
    .AddCheck<ReviewsRedisHealthCheck>("redis-cache", tags: ["cache", "redis", "ready"], timeout: TimeSpan.FromSeconds(3))
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["messaging", "ready"], timeout: TimeSpan.FromSeconds(3));
builder.Services.AddGrpcHealthChecks();

builder.Services.AddApplication(builder.Configuration["LuckyPenny:LicenseKey"]);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IIntegrationEventHandler, CatalogIntegrationEventHandler>();
builder.Services.AddSingleton(new RabbitMqConsumerOptions(RabbitMqTopology.ReviewsQueue));
builder.Services.AddHostedService<RabbitMqConsumer>();
builder.Services.AddWebApi();
builder.Services.AddGrpc(options => options.Interceptors.Add<LoggingInterceptor>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.Use(async (context, next) =>
{
    var metrics = context.RequestServices.GetRequiredService<ReviewMetrics>();
    var timer = System.Diagnostics.Stopwatch.StartNew();
    var success = false;
    metrics.Enter();
    try
    {
        await next();
        success = context.Response.StatusCode is >= 200 and < 400;
    }
    finally
    {
        metrics.Exit();
        metrics.Record(context.GetEndpoint()?.DisplayName ?? "unmatched", success, timer.Elapsed.TotalMilliseconds);
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGrpcService<ReviewsGrpcService>();
app.MapGrpcHealthChecksService();
app.MapDefaultEndpoints();

app.Run();
