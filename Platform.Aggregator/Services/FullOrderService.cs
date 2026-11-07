using System.Collections.Concurrent;
using System.Diagnostics;
using Platform.Aggregator.Clients;

namespace Platform.Aggregator.Services;

public sealed class FullOrderService(
    OrdersClient orders,
    CatalogClient catalog,
    ReviewsClient reviews,
    ILogger<FullOrderService> logger)
{
    public async Task<FullOrderDto?> GetFullOrderAsync(long orderId, CancellationToken cancellationToken)
    {
        var order = await InvokeAsync("orders-api", () => orders.GetOrderAsync(orderId, cancellationToken), cancellationToken);
        if (order is null) return null;

        var productIds = order.Items.Select(item => item.ProductId).Distinct().ToArray();
        var warnings = new ConcurrentBag<string>();
        var productTasks = productIds.Select(id => SafeAsync(
            "catalog-api", () => catalog.GetProductAsync(id, cancellationToken), warnings, cancellationToken));
        var reviewTasks = productIds.Select(id => SafeAsync(
            "reviews-api", () => reviews.GetForProductAsync(id, cancellationToken), warnings, cancellationToken));

        var productsTask = Task.WhenAll(productTasks);
        var reviewsTask = Task.WhenAll(reviewTasks);
        await Task.WhenAll(productsTask, reviewsTask);

        var productResults = await productsTask;
        var reviewResults = await reviewsTask;

        return new FullOrderDto(order,
            productResults.Where(product => product is not null).Cast<ProductDto>().ToArray(),
            reviewResults.SelectMany(productReviews => productReviews ?? []).ToArray(),
            warnings.Distinct().ToArray());
    }

    private async Task<T?> SafeAsync<T>(string service, Func<Task<T?>> call,
        ConcurrentBag<string> warnings, CancellationToken cancellationToken)
    {
        try { return await InvokeAsync(service, call, cancellationToken); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "{Service} is unavailable", service);
            warnings.Add($"{service} is unavailable");
            return default;
        }
    }

    private async Task<T?> InvokeAsync<T>(string service, Func<Task<T?>> call, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        logger.LogInformation("Calling {Service}", service);
        var result = await call();
        logger.LogInformation("{Service} responded in {ElapsedMs} ms", service, timer.ElapsedMilliseconds);
        return result;
    }
}

public sealed record FullOrderDto(OrderDto Order, IReadOnlyList<ProductDto> Products,
    IReadOnlyList<ReviewDto> Reviews, IReadOnlyList<string> Warnings);
