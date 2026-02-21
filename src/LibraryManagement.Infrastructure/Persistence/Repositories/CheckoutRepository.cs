using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence.Repositories;

public class CheckoutRepository : GenericRepository<CheckoutRecord>, ICheckoutRepository
{
    public CheckoutRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CheckoutRecord>> GetActiveByPatronIdAsync(Guid patronId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Book)
            .Where(c => c.PatronId == patronId && c.Status == CheckoutStatus.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CheckoutRecord>> GetAllByPatronIdAsync(Guid patronId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Book)
            .Where(c => c.PatronId == patronId)
            .OrderByDescending(c => c.CheckedOutAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CheckoutRecord>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Book)
            .Include(c => c.Patron)
            .Where(c => c.Status == CheckoutStatus.Active && c.DueDate < DateTime.UtcNow)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<CheckoutRecord?> GetActiveByBookAndPatronAsync(Guid bookId, Guid patronId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.BookId == bookId && c.PatronId == patronId && c.Status == CheckoutStatus.Active, cancellationToken);
    }
}
