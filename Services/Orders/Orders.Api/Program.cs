using Orders.Api.ExceptionHandling;
using Orders.Api.Infrastructure;
using Orders.Bll.Mapping;
using Orders.Bll.Services;
using Orders.Dal;
using Orders.Dal.Interfaces;
using Orders.Api.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(
    endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddRedisDistributedCache("redis");
builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);

var connectionString = builder.Configuration.GetConnectionString("OrdersDb")
    ?? throw new InvalidOperationException("Connection string 'OrdersDb' is missing");

builder.Services.AddScoped<IUnitOfWork>(_ => new UnitOfWork(connectionString));
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IHostedService>(sp => new OrdersDatabaseBootstrapper(
    connectionString, sp.GetRequiredService<ILogger<OrdersDatabaseBootstrapper>>()));

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
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGrpcService<OrdersGrpcService>();
app.MapDefaultEndpoints();
app.Run();
