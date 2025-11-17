namespace Domain.ValueObjects;

public class IdempotencyRecord
{
    public string Key { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public bool IsExpired(TimeSpan expirationWindow)
    {
        return DateTime.UtcNow - CreatedAt > expirationWindow;
    }
}

