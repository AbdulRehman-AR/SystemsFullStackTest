using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class StudentRepository : Repository<Student>, IRepository<Student>
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Parent)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}

