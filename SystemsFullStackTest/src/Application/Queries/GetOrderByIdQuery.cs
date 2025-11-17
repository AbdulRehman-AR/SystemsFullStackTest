using Application.DTOs;
using MediatR;

namespace Application.Queries;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public Guid OrderId { get; set; }
}

