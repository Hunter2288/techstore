using Grpc.Core;
using MediatR;
using Reviews.Application.Reviews.Queries.GetReviewsList;
using Reviews.Grpc;

namespace Reviews.WebApi.Grpc;

public sealed class ReviewsGrpcService(ISender sender) : ReviewsService.ReviewsServiceBase
{
    public override async Task<ReviewsReply> GetForProduct(
        GetForProductRequest request, ServerCallContext context)
    {
        if (request.ProductId <= 0 || request.ProductId > int.MaxValue)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProductId must be positive"));

        var page = await sender.Send(new GetReviewsListQuery(1, 100, checked((int)request.ProductId)),
            context.CancellationToken);
        var reply = new ReviewsReply();
        reply.Reviews.AddRange(page.Items.Select(review => new ReviewReply
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = review.ProductName,
            Rating = review.Rating,
            Title = review.Title,
            Text = review.Text
        }));
        return reply;
    }
}
