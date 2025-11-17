using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CanteenRepository : Repository<Canteen>, IRepository<Canteen>
{
    public CanteenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Canteen?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}

