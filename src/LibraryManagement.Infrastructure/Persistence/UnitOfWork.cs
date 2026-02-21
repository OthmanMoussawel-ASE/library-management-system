using System.Collections.Concurrent;
using LibraryManagement.Domain.Common;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Infrastructure.Persistence.Repositories;
using MediatR;

namespace LibraryManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IMediator _mediator;
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    private IBookRepository? _books;
    private ICheckoutRepository? _checkouts;
    private IPatronRepository? _patrons;

    public UnitOfWork(AppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public IBookRepository Books => _books ??= new BookRepository(_context);
    public ICheckoutRepository Checkouts => _checkouts ??= new CheckoutRepository(_context);
    public IPatronRepository Patrons => _patrons ??= new PatronRepository(_context);

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var typeName = typeof(T).Name;
        return (IGenericRepository<T>)_repositories.GetOrAdd(typeName, _ => new GenericRepository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
