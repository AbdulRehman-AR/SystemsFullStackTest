namespace Domain.Entities;

public class Canteen
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<CanteenSchedule> Schedules { get; set; } = new List<CanteenSchedule>();
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    
    public TimeSpan? GetCutoffTimeForDay(DayOfWeek dayOfWeek)
    {
        return Schedules
            .FirstOrDefault(s => s.DayOfWeek == dayOfWeek)
            ?.CutoffTime;
    }
    
    public bool IsOpenOnDay(DayOfWeek dayOfWeek)
    {
        return Schedules.Any(s => s.DayOfWeek == dayOfWeek);
    }
}

public class CanteenSchedule
{
    public Guid Id { get; set; }
    public Guid CanteenId { get; set; }
    public Canteen Canteen { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan CutoffTime { get; set; }
}

