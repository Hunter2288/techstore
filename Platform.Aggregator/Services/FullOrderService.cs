using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Catalog.Grpc;
using Grpc.Core;
using Microsoft.Extensions.Caching.Distributed;
using Orders.Grpc;
using Reviews.Grpc;

namespace Platform.Aggregator.Services;

public sealed class FullOrderService(
    OrdersService.OrdersServiceClient orders,
    CatalogService.CatalogServiceClient catalog,
    ReviewsService.ReviewsServiceClient reviews,
    IDistributedCache cache,
    ILogger<FullOrderService> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<FullOrderDto?> GetFullOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var cacheKey = $"aggregator:order:{orderId}:full";
        try
        {
            var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (cached is not null)
            {
                logger.LogInformation("Aggregator cache hit {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<FullOrderDto>(cached, JsonOptions);
            }
            logger.LogInformation("Aggregator cache miss {CacheKey}", cacheKey);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Redis read failed for {CacheKey}; aggregating live data", cacheKey);
        }

        OrderReply order;
        try
        {
            logger.LogInformation("Calling {Service} for {OrderId}", "orders-api", orderId);
            order = await orders.GetOrderAsync(new GetOrderRequest { Id = orderId }, cancellationToken: cancellationToken);
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
        {
            return null;
        }

        var productIds = order.Items.Select(item => item.ProductId).Distinct().ToArray();
        var warnings = new ConcurrentBag<string>();
        var productsTask = SafeAsync("catalog-api", async () =>
        {
            var result = await catalog.GetProductsByIdsAsync(
                new GetProductsByIdsRequest { Ids = { productIds } }, cancellationToken: cancellationToken);
            return result.Products.Select(product => new ProductDto(product.Id, product.Sku,
                product.Name, decimal.Parse(product.Price, CultureInfo.InvariantCulture),
                product.AvailableStock)).ToArray();
        }, static () => Array.Empty<ProductDto>(), warnings, cancellationToken);

        var reviewTasks = productIds.Select(productId => SafeAsync("reviews-api", async () =>
        {
            var result = await reviews.GetForProductAsync(
                new GetForProductRequest { ProductId = productId }, cancellationToken: cancellationToken);
            return result.Reviews.Select(review => new ReviewDto(review.Id, review.ProductId,
                review.ProductName, review.Rating, review.Title, review.Text)).ToArray();
        }, static () => Array.Empty<ReviewDto>(), warnings, cancellationToken));

        var reviewsTask = Task.WhenAll(reviewTasks);
        await Task.WhenAll(productsTask, reviewsTask);
        var productResults = await productsTask;
        var reviewResults = await reviewsTask;
        var resultDto = new FullOrderDto(
            new OrderDto(order.Id, order.CustomerId, order.Status,
                decimal.Parse(order.TotalAmount, CultureInfo.InvariantCulture),
                DateTime.Parse(order.CreatedAt, CultureInfo.InvariantCulture),
                order.Items.Select(item => new OrderItemDto(item.ProductId, item.ProductName,
                    decimal.Parse(item.UnitPrice, CultureInfo.InvariantCulture), item.Quantity,
                    decimal.Parse(item.LineTotal, CultureInfo.InvariantCulture))).ToArray()),
            productResults,
            reviewResults.SelectMany(items => items).ToArray(),
            warnings.Distinct().ToArray());

        if (resultDto.Warnings.Count == 0)
        {
            try
            {
                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(resultDto, JsonOptions),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(45)
                    }, cancellationToken);
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Redis write failed for {CacheKey}", cacheKey);
            }
        }
        return resultDto;
    }

    private async Task<T> SafeAsync<T>(string service, Func<Task<T>> call, Func<T> fallback,
        ConcurrentBag<string> warnings, CancellationToken cancellationToken)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var value = await call();
            logger.LogInformation("{Service} responded in {ElapsedMs} ms", service, timer.ElapsedMilliseconds);
            return value;
        }
        catch (RpcException exception) when (!cancellationToken.IsCancellationRequested &&
                                             exception.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded)
        {
            logger.LogWarning(exception, "{Service} gRPC call failed with {Status}", service, exception.StatusCode);
            warnings.Add($"{service} is unavailable");
            return fallback();
        }
    }
}

public sealed record FullOrderDto(OrderDto Order, IReadOnlyList<ProductDto> Products,
    IReadOnlyList<ReviewDto> Reviews, IReadOnlyList<string> Warnings);
public sealed record OrderDto(long OrderId, long CustomerId, string Status, decimal TotalAmount,
    DateTime CreatedAt, IReadOnlyList<OrderItemDto> Items);
public sealed record OrderItemDto(long ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public sealed record ProductDto(long ProductId, string Sku, string Name, decimal Price, int StockQuantity);
public sealed record ReviewDto(string Id, int ProductId, string ProductName, int Rating, string Title, string Text);
