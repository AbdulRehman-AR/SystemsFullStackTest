namespace Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

