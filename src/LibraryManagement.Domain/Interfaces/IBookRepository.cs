using LibraryManagement.Domain.Entities;

namespace LibraryManagement.Domain.Interfaces;

public interface IBookRepository : IGenericRepository<Book>
{
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetAllWithAuthorsAsync(CancellationToken cancellationToken = default);
}
