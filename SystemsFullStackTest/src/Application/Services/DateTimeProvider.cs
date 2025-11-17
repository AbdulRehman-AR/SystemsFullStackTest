using Application.Interfaces;

namespace Application.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}

