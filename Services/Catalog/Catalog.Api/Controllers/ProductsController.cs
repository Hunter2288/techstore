using Catalog.Bll.Dto;
using Catalog.Bll.Services;
using Catalog.Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsController(IProductService service) : ControllerBase
    {
        // метадані пагінації: у тілі (items, totalCount, page, pageSize, totalPages, hasNext, hasPrevious)
        // і дублюються в заголовках X-Total-Count та X-Total-Pages
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<ProductDto>>> GetList(
            [FromQuery] ProductQuery query, CancellationToken ct)
        {
            var result = await service.GetListAsync(query, ct);
            Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
            Response.Headers["X-Total-Pages"] = result.TotalPages.ToString();
            return Ok(result);
        }

        [HttpGet("{id:long}")]                                            // Eager Loading
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductCardDto>> GetById(long id, CancellationToken ct)
            => Ok(await service.GetByIdAsync(id, ct));

        [HttpGet("recent")]                                               // LINQ через M:N
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetRecent(
            [FromQuery] string category, [FromQuery] int days = 30, CancellationToken ct = default)
            => Ok(await service.GetRecentlyAddedAsync(category, days, ct));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ProductDto>> Create(
            CreateProductDto input,
            [FromServices] IValidator<CreateProductDto> validator,
            CancellationToken ct)
        {
            var validation = await validator.ValidateAsync(input, ct);
            if (!validation.IsValid)
                return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

            var created = await service.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
        }

        [HttpPut("{id:long}/price")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePrice(long id, ChangePriceDto input, CancellationToken ct)
        {
            await service.ChangePriceAsync(id, input.Price, ct);
            return NoContent();
        }

        [HttpPost("{id:long}/stock/remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RemoveStock(long id, RemoveStockDto input, CancellationToken ct)
        {
            await service.RemoveStockAsync(id, input.Quantity, ct);
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
