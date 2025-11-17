using Application.Commands;
using Application.DTOs;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;
    private readonly IWebHostEnvironment _environment;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _environment = environment;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto orderDto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateOrderCommand
            {
                Order = orderDto,
                IdempotencyKey = idempotencyKey
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
        }
        catch (Domain.Exceptions.OrderValidationException ex)
        {
            _logger.LogWarning("Order validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            var errorMessage = _environment.IsDevelopment()
                ? ex.Message
                : "An error occurred while creating the order";
            return StatusCode(500, new { error = errorMessage });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery { OrderId = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}

