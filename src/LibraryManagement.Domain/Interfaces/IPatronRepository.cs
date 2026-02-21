using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Domain.Interfaces;

public interface IPatronRepository : IGenericRepository<Patron>
{
    Task<Patron?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
