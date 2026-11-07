using System.Net.Http.Json;

namespace Platform.Aggregator.Clients;

public sealed class ReviewsClient(HttpClient http)
{
    public async Task<IReadOnlyList<ReviewDto>?> GetForProductAsync(long productId, CancellationToken cancellationToken)
    {
        var result = await http.GetFromJsonAsync<PagedReviews>(
            $"/api/reviews?PageNumber=1&PageSize=100&ProductId={productId}", cancellationToken);
        return result?.Items ?? [];
    }
}

public sealed record PagedReviews(IReadOnlyList<ReviewDto> Items);
public sealed record ReviewDto(string Id, int ProductId, string ProductName, int Rating, string Title, string Text);
