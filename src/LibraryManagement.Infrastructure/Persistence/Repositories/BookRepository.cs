using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence.Repositories;

public class BookRepository : GenericRepository<Book>, IBookRepository
{
    public BookRepository(AppDbContext context) : base(context) { }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.ISBN == isbn && !b.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Include(b => b.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .Where(b => !b.IsDeleted && (
                b.Title.ToLower().Contains(term) ||
                (b.ISBN != null && b.ISBN.ToLower().Contains(term)) ||
                b.Author.FirstName.ToLower().Contains(term) ||
                b.Author.LastName.ToLower().Contains(term)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
            .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetAllWithAuthorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Author)
            .Where(b => !b.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
