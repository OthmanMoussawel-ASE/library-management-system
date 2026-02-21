using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Domain.Interfaces;

public interface ICheckoutRepository : IGenericRepository<CheckoutRecord>
{
    Task<IReadOnlyList<CheckoutRecord>> GetActiveByPatronIdAsync(Guid patronId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CheckoutRecord>> GetAllByPatronIdAsync(Guid patronId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CheckoutRecord>> GetOverdueAsync(CancellationToken cancellationToken = default);
    Task<CheckoutRecord?> GetActiveByBookAndPatronAsync(Guid bookId, Guid patronId, CancellationToken cancellationToken = default);
}
