namespace Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CanteenId { get; set; }
    public DateTime FulfilmentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemDetailDto> Items { get; set; } = new();
}

public class OrderItemDetailDto
{
    public Guid MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

