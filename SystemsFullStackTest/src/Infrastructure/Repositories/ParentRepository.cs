using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ParentRepository : Repository<Parent>, IParentRepository
{
    public ParentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Parent?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }
}

