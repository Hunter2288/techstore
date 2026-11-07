using System.Text.Json.Serialization;
using Platform.Aggregator.Clients;
using Platform.Aggregator.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddHttpClient<OrdersClient>(client => client.BaseAddress = new Uri("https+http://orders-api"));
builder.Services.AddHttpClient<CatalogClient>(client => client.BaseAddress = new Uri("https+http://catalog-api"));
builder.Services.AddHttpClient<ReviewsClient>(client => client.BaseAddress = new Uri("https+http://reviews-api"));
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
