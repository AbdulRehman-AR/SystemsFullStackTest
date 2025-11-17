namespace Application.DTOs;

public class CreateOrderDto
{
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CanteenId { get; set; }
    public DateTime FulfilmentDate { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
}

