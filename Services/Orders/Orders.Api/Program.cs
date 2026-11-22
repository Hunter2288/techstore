using Orders.Api.ExceptionHandling;
using Orders.Api.Infrastructure;
using Orders.Bll.Mapping;
using Orders.Bll.Services;
using Orders.Dal;
using Orders.Dal.Interfaces;
using Orders.Api.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orders.Api;
using Orders.Api.HealthChecks;
using Orders.Api.IntegrationEvents;
using Platform.EventBus;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(
    endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddRedisDistributedCache("redis");
builder.AddRabbitMQClient("messaging");
builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);
builder.Services.AddSingleton<OrderMetrics>();

var connectionString = builder.Configuration.GetConnectionString("OrdersDb")
    ?? throw new InvalidOperationException("Connection string 'OrdersDb' is missing");

builder.Services.AddSingleton<OrdersDatabaseHealthCheck>();
builder.Services.AddHealthChecks()
    .AddCheck<OrdersDatabaseHealthCheck>("orders-db", tags: ["database", "sql", "ready"], timeout: TimeSpan.FromSeconds(5))
    .AddCheck<OrdersRedisHealthCheck>("redis-cache", tags: ["cache", "redis", "ready"], timeout: TimeSpan.FromSeconds(3))
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["messaging", "ready"], timeout: TimeSpan.FromSeconds(3));
builder.Services.AddGrpcHealthChecks();

builder.Services.AddScoped<IUnitOfWork>(_ => new UnitOfWork(connectionString));
builder.Services.AddScoped<IIntegrationEventHandler, ProductSnapshotHandler>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IHostedService>(sp => new OrdersDatabaseBootstrapper(
    connectionString, sp.GetRequiredService<ILogger<OrdersDatabaseBootstrapper>>()));
builder.Services.AddSingleton(new RabbitMqConsumerOptions(RabbitMqTopology.OrdersQueue));
builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
    cfg.AddProfile<OrdersProfile>();
});

builder.Services.AddControllers();
builder.Services.AddGrpc(options => options.Interceptors.Add<LoggingInterceptor>());
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.Use(async (context, next) =>
{
    var timer = System.Diagnostics.Stopwatch.StartNew();
    try { await next(); }
    finally { app.Services.GetRequiredService<OrderMetrics>().Record(
        context.GetEndpoint()?.DisplayName ?? "unmatched", context.Response.StatusCode is >= 200 and < 400, timer.Elapsed.TotalMilliseconds); }
});
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGrpcService<OrdersGrpcService>();
app.MapGrpcHealthChecksService();
app.MapDefaultEndpoints();
app.Run();
