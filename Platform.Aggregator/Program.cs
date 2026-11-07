using System.Text.Json.Serialization;
using Catalog.Grpc;
using Orders.Grpc;
using Reviews.Grpc;
using Platform.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddRedisDistributedCache("redis");
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddGrpcClient<OrdersService.OrdersServiceClient>(options =>
    options.Address = new Uri("https+http://orders-api"));
builder.Services.AddGrpcClient<CatalogService.CatalogServiceClient>(options =>
    options.Address = new Uri("https+http://catalog-api"));
builder.Services.AddGrpcClient<ReviewsService.ReviewsServiceClient>(options =>
    options.Address = new Uri("https+http://reviews-api"));
builder.Services.AddScoped<FullOrderService>();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapGet("/api/aggregator/orders/{id:long}/full", async (
    long id, FullOrderService service, CancellationToken cancellationToken) =>
{
    var result = await service.GetFullOrderAsync(id, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
});
app.MapDefaultEndpoints();
app.Run();
