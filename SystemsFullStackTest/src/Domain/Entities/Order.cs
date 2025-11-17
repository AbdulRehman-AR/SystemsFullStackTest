namespace Domain.Entities;

public enum OrderStatus
{
    Placed,
    Confirmed,
    Fulfilled,
    Cancelled
}

public class Order
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = null!;
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public Guid CanteenId { get; set; }
    public Canteen Canteen { get; set; } = null!;
    public DateTime FulfilmentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? IdempotencyKey { get; set; }
    
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    
    public void Confirm()
    {
        if (Status != OrderStatus.Placed)
            throw new InvalidOperationException("Only placed orders can be confirmed");
        
        Status = OrderStatus.Confirmed;
    }
    
    public decimal CalculateTotal()
    {
        return Items.Sum(item => item.Quantity * item.UnitPrice);
    }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

