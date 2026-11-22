using Catalog.Api.ExceptionHandling;
using Catalog.Api.Grpc;
using Catalog.Bll.Caching;
using Catalog.Bll.Mapping;
using Catalog.Bll.Services;
using Catalog.Bll.Validators;
using Catalog.Dal;
using Catalog.Dal.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Catalog.Api;
using Catalog.Api.HealthChecks;
using Platform.EventBus;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(
    endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddRedisDistributedCache("redis");
builder.AddRabbitMQClient("messaging");
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);

var catalogConnectionString = builder.Configuration.GetConnectionString("CatalogDb")
    ?? throw new InvalidOperationException("Connection string 'CatalogDb' is missing");
builder.Services.AddSingleton<CatalogDatabaseHealthCheck>();
builder.Services.AddHealthChecks()
    .AddCheck<CatalogDatabaseHealthCheck>("catalog-db", tags: ["database", "sql", "ready"], timeout: TimeSpan.FromSeconds(5))
    .AddCheck<CatalogRedisHealthCheck>("redis-cache", tags: ["cache", "redis", "ready"], timeout: TimeSpan.FromSeconds(3))
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["messaging", "ready"], timeout: TimeSpan.FromSeconds(3));
builder.Services.AddGrpcHealthChecks();
builder.Services.AddSingleton<CacheMetrics>();
builder.Services.AddSingleton<CatalogMetrics>();

builder.Services.AddDbContext<CatalogDbContext>(o =>
    o.UseSqlServer(catalogConnectionString));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IProductService, CachedProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
    cfg.AddProfile<CatalogProfile>();
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

builder.Services.AddControllers();
builder.Services.AddGrpc(options => options.Interceptors.Add<LoggingInterceptor>());
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
    if (!app.Environment.IsProduction())
        await DbInitializer.SeedAsync(db);
}
app.UseMiddleware<CorrelationIdMiddleware>();
app.Use(async (context, next) =>
{
    var timer = System.Diagnostics.Stopwatch.StartNew();
    try { await next(); }
    finally { app.Services.GetRequiredService<CatalogMetrics>().Record(
        context.GetEndpoint()?.DisplayName ?? "unmatched", context.Response.StatusCode is >= 200 and < 400, timer.Elapsed.TotalMilliseconds); }
});
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGrpcService<CatalogGrpcService>();
app.MapGrpcHealthChecksService();
app.MapDefaultEndpoints();
app.Run();
