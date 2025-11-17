using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
}

