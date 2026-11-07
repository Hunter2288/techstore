using Reviews.Application;
using Reviews.Infrastructure;
using Reviews.WebApi;
using Reviews.WebApi.Middleware;
using Reviews.WebApi.Health;
using Reviews.WebApi.Grpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.WebHost.ConfigureKestrel(options => options.ConfigureEndpointDefaults(
    endpoint => endpoint.Protocols = HttpProtocols.Http1AndHttp2));
builder.AddRedisDistributedCache("redis");
builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);

builder.Services.AddApplication(builder.Configuration["LuckyPenny:LicenseKey"]);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddWebApi();
builder.Services.AddGrpc(options => options.Interceptors.Add<LoggingInterceptor>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb");

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGrpcService<ReviewsGrpcService>();
app.MapDefaultEndpoints();

app.Run();
