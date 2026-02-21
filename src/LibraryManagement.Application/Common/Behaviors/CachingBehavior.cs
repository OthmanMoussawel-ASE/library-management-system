using LibraryManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LibraryManagement.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICachedQuery cachedQuery)
            return await next(cancellationToken);

        var cachedResult = await _cacheService.GetAsync<TResponse>(cachedQuery.CacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            _logger.LogInformation("Cache hit for {CacheKey}", cachedQuery.CacheKey);
            return cachedResult;
        }

        var response = await next(cancellationToken);

        await _cacheService.SetAsync(cachedQuery.CacheKey, response, cachedQuery.CacheDuration, cancellationToken);
        _logger.LogInformation("Cache set for {CacheKey}", cachedQuery.CacheKey);

        return response;
    }
}
