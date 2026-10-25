using Microsoft.AspNetCore.Mvc;
using Orders.Bll.Dto;
using Orders.Bll.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/orders/{orderId:long}/payments")]
[Produces("application/json")]
public class PaymentsController(IPaymentService payments) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetByOrder(long orderId, CancellationToken ct)
        => Ok(await payments.GetByOrderAsync(orderId, ct));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentDto>> Register(long orderId, RegisterPaymentDto input, CancellationToken ct)
    {
        var created = await payments.RegisterAsync(orderId, input, ct);
        return CreatedAtAction(nameof(GetByOrder), new { orderId }, created);
    }
}
