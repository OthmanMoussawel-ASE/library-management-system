namespace LibraryManagement.Application.Common.Interfaces;

public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}
