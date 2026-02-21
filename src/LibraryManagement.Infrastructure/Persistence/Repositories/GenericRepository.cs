using LibraryManagement.Domain.Common;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), specification)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(ISpecification<T>? specification = null, CancellationToken cancellationToken = default)
    {
        if (specification is null)
            return await _dbSet.CountAsync(cancellationToken);

        return await SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), specification)
            .CountAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
            _dbSet.Attach(entity);
        entry.State = EntityState.Modified;
    }

    public virtual void Delete(T entity)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = true;
            softDeletable.DeletedAt = DateTime.UtcNow;
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
                _dbSet.Attach(entity);
            entry.State = EntityState.Modified;
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
