using Catalog.Bll.Services;
using Catalog.Domain.Exceptions;
using Catalog.Grpc;
using Grpc.Core;

namespace Catalog.Api.Grpc;

public sealed class CatalogGrpcService(IProductService products) : CatalogService.CatalogServiceBase
{
    public override async Task<ProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be positive"));
        try { return Map(await products.GetByIdAsync(request.Id, context.CancellationToken)); }
        catch (EntityNotFoundException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }

    public override async Task<ProductsReply> GetProductsByIds(
        GetProductsByIdsRequest request, ServerCallContext context)
    {
        if (request.Ids.Any(id => id <= 0))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Every product id must be positive"));

        var reply = new ProductsReply();
        foreach (var id in request.Ids.Distinct())
        {
            try { reply.Products.Add(Map(await products.GetByIdAsync(id, context.CancellationToken))); }
            catch (EntityNotFoundException) { }
        }
        return reply;
    }

    private static ProductReply Map(Catalog.Bll.Dto.ProductCardDto product) => new()
    {
        Id = product.ProductId,
        Name = product.Name,
        Sku = product.Sku,
        Price = product.Price.ToString(System.Globalization.CultureInfo.InvariantCulture),
        AvailableStock = product.StockQuantity
    };
}
