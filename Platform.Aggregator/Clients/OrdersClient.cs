using System.Net;
using System.Net.Http.Json;

namespace Platform.Aggregator.Clients;

public sealed class OrdersClient(HttpClient http)
{
    public async Task<OrderDto?> GetOrderAsync(long id, CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync($"/api/orders/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken: cancellationToken);
    }
}

public sealed record OrderDto(long OrderId, long CustomerId, string Status, decimal TotalAmount,
    DateTime CreatedAt, IReadOnlyList<OrderItemDto> Items);
public sealed record OrderItemDto(long ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
