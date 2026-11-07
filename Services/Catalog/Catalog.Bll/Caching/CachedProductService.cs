using System.Text.Json;
using Catalog.Bll.Dto;
using Catalog.Bll.Services;
using Catalog.Domain.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Catalog.Bll.Caching;

public sealed class CachedProductService(
    ProductService inner,
    IMemoryCache memory,
    IDistributedCache distributed,
    CacheMetrics metrics,
    ILogger<CachedProductService> logger) : IProductService
{
    private static readonly TimeSpan LocalTtl = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan DistributedTtl = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ProductCardDto> GetByIdAsync(long id, CancellationToken ct)
    {
        var key = Key(id);
        if (memory.TryGetValue(key, out ProductCardDto? local) && local is not null)
        {
            metrics.Hit("L1");
            logger.LogDebug("Catalog cache hit {CacheLevel} {CacheKey} {TtlSeconds}",
                "memory", key, LocalTtl.TotalSeconds);
            return local;
        }
        metrics.Miss("L1");
        logger.LogDebug("Catalog cache miss {CacheLevel} {CacheKey}", "memory", key);

        try
        {
            var json = await distributed.GetStringAsync(key, ct);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var shared = JsonSerializer.Deserialize<ProductCardDto>(json, JsonOptions);
                if (shared is not null)
                {
                    metrics.Hit("L2");
                    logger.LogDebug("Catalog cache hit {CacheLevel} {CacheKey} {TtlSeconds} {PayloadBytes}",
                        "redis", key, DistributedTtl.TotalSeconds, System.Text.Encoding.UTF8.GetByteCount(json));
                    SetLocal(key, shared);
                    return shared;
                }
            }
            metrics.Miss("L2");
            logger.LogDebug("Catalog cache miss {CacheLevel} {CacheKey}", "redis", key);
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Redis read failed for {CacheKey}; falling back to SQL", key);
        }

        var product = await inner.GetByIdAsync(id, ct);
        SetLocal(key, product);
        try
        {
            var json = JsonSerializer.Serialize(product, JsonOptions);
            logger.LogDebug("Catalog cache populated {CacheLevel} {CacheKey} {TtlSeconds} {PayloadBytes}",
                "memory", key, LocalTtl.TotalSeconds, System.Text.Encoding.UTF8.GetByteCount(json));
            await distributed.SetStringAsync(key, json,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DistributedTtl }, ct);
            logger.LogDebug("Catalog cache populated {CacheLevel} {CacheKey} {TtlSeconds} {PayloadBytes}",
                "redis", key, DistributedTtl.TotalSeconds, System.Text.Encoding.UTF8.GetByteCount(json));
        }
        catch (Exception exception) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Redis write failed for {CacheKey}; product remains available from SQL", key);
        }
        return product;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto input, CancellationToken ct)
        => await InvalidateAfterAsync(() => inner.CreateAsync(input, ct), ct);

    public Task ChangePriceAsync(long id, decimal newPrice, CancellationToken ct)
        => InvalidateAfterAsync(() => inner.ChangePriceAsync(id, newPrice, ct), id, ct);

    public Task RemoveStockAsync(long id, int quantity, CancellationToken ct)
        => InvalidateAfterAsync(() => inner.RemoveStockAsync(id, quantity, ct), id, ct);

    public Task DeleteAsync(long id, CancellationToken ct)
        => InvalidateAfterAsync(() => inner.DeleteAsync(id, ct), id, ct);

    public Task<PagedResult<ProductDto>> GetListAsync(ProductQuery query, CancellationToken ct)
        => inner.GetListAsync(query, ct);

    public Task<IReadOnlyList<ProductDto>> GetRecentlyAddedAsync(string category, int days, CancellationToken ct)
        => inner.GetRecentlyAddedAsync(category, days, ct);

    private async Task<T> InvalidateAfterAsync<T>(Func<Task<T>> write, CancellationToken ct)
    {
        var result = await write();
        await InvalidateAsync(null, ct);
        return result;
    }

    private async Task InvalidateAfterAsync(Func<Task> write, long id, CancellationToken ct)
    {
        await write();
        await InvalidateAsync(id, ct);
    }

    private async Task InvalidateAsync(long? id, CancellationToken ct)
    {
        if (id is long productId) memory.Remove(Key(productId));
        metrics.Invalidated();
        logger.LogInformation("Catalog cache invalidated for {ProductId} after product write", id);
        if (id is long keyId)
        {
            try { await distributed.RemoveAsync(Key(keyId), ct); }
            catch (Exception exception) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Redis invalidation failed for {CacheKey}", Key(keyId));
            }
        }
    }

    private void SetLocal(string key, ProductCardDto value)
        => memory.Set(key, value, new MemoryCacheEntryOptions
        {
            Size = 1,
            AbsoluteExpirationRelativeToNow = LocalTtl,
            SlidingExpiration = TimeSpan.FromSeconds(20),
            Priority = CacheItemPriority.High
        });

    private static string Key(long id) => $"catalog:product:{id}";
}
