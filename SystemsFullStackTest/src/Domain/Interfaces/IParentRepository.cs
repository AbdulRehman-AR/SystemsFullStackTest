using Domain.Entities;

namespace Domain.Interfaces;

public interface IParentRepository : IRepository<Parent>
{
    Task<Parent?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

