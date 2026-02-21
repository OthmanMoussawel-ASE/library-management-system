namespace LibraryManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBookRepository Books { get; }
    ICheckoutRepository Checkouts { get; }
    IPatronRepository Patrons { get; }
    IGenericRepository<T> Repository<T>() where T : Common.BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
