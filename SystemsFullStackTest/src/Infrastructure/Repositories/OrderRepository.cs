using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public override async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
            .Include(o => o.Parent)
            .Include(o => o.Student)
            .Include(o => o.Canteen)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}

