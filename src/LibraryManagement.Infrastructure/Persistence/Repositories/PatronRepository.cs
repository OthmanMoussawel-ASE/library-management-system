using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence.Repositories;

public class PatronRepository : GenericRepository<Patron>, IPatronRepository
{
    public PatronRepository(AppDbContext context) : base(context) { }

    public async Task<Patron?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}
