using Application.DTOs;
using Application.Queries;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
            return null;

        return new OrderDto
        {
            Id = order.Id,
            ParentId = order.ParentId,
            StudentId = order.StudentId,
            CanteenId = order.CanteenId,
            FulfilmentDate = order.FulfilmentDate,
            CreatedAt = order.CreatedAt,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item => new OrderItemDetailDto
            {
                MenuItemId = item.MenuItemId,
                MenuItemName = item.MenuItem.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}

