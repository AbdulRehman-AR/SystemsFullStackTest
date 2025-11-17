namespace Domain.Entities;

public class Parent
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    
    public ICollection<Student> Students { get; set; } = new List<Student>();
    
    public void DebitWallet(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));
        
        if (WalletBalance < amount)
            throw new InvalidOperationException("Insufficient wallet balance");
        
        WalletBalance -= amount;
    }
}

