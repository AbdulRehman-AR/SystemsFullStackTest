namespace Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = null!;
    public string? Allergen { get; set; }
}

