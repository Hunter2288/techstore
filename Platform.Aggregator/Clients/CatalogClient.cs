using System.Net.Http.Json;

namespace Platform.Aggregator.Clients;

public sealed class CatalogClient(HttpClient http)
{
    public async Task<ProductDto?> GetProductAsync(long id, CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync($"/api/products/{id}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: cancellationToken);
    }
}

public sealed record ProductDto(long ProductId, string Sku, string Name, decimal Price, int StockQuantity);
