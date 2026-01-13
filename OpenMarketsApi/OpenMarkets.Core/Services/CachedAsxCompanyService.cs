using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OpenMarkets.Core.DomainModels;

namespace OpenMarkets.Core.Services;

public class CachedAsxCompanyService : IAsxCompanyService
{
    private readonly IAsxCompanyService _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedAsxCompanyService> _logger;
    private const string CacheKeyPrefix = "AsxCompany_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = CacheDuration
    };

    public CachedAsxCompanyService(
        IAsxCompanyService inner,
        IDistributedCache cache,
        ILogger<CachedAsxCompanyService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AsxCompany> GetCompanyByCodeAsync(string asxCode, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{asxCode.ToUpperInvariant()}";

        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes != null)
        {
            try
            {
                var cachedCompany = JsonSerializer.Deserialize<AsxCompany>(cachedBytes);
                if (cachedCompany != null)
                {
                    _logger.LogInformation("Cache hit for ASX code '{AsxCode}'", asxCode);
                    return cachedCompany;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached company for ASX code '{AsxCode}'", asxCode);
            }
        }

        _logger.LogInformation("Cache miss for ASX code '{AsxCode}'", asxCode);

        var company = await _inner.GetCompanyByCodeAsync(asxCode, cancellationToken);

        var serializedCompany = JsonSerializer.SerializeToUtf8Bytes(company);
        await _cache.SetAsync(cacheKey, serializedCompany, CacheOptions, cancellationToken);

        _logger.LogInformation("Cached company with ASX code '{AsxCode}' for {Duration}", asxCode, CacheDuration);

        return company;
    }
}
