namespace Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid CanteenId { get; set; }
    public Canteen Canteen { get; set; } = null!;
    public int? DailyStockCount { get; set; }
    public ICollection<string> AllergenTags { get; set; } = new List<string>();
    
    public bool HasAllergen(string allergen)
    {
        return AllergenTags.Contains(allergen, StringComparer.OrdinalIgnoreCase);
    }
    
    public bool IsInStock(int requestedQuantity)
    {
        if (DailyStockCount == null)
            return true;
        
        return DailyStockCount >= requestedQuantity;
    }
    
    public void DecrementStock(int quantity)
    {
        if (DailyStockCount == null)
            return;
        
        if (DailyStockCount < quantity)
            throw new InvalidOperationException("Insufficient stock");
        
        DailyStockCount -= quantity;
    }
}

