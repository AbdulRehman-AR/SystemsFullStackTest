using Application.DTOs;
using MediatR;

namespace Application.Commands;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public CreateOrderDto Order { get; set; } = null!;
    public string? IdempotencyKey { get; set; }
}

