using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OpenMarkets.Core.DomainModels;
using OpenMarkets.Core.Interfaces;

namespace OpenMarkets.Infrastructure.Csv;

public class CachedAsxCsvDataProvider : IAsxCsvDataProvider
{
    private readonly IAsxCsvDataProvider _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedAsxCsvDataProvider> _logger;
    private const string CacheKey = "AsxCompanies_All";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = CacheDuration
    };

    public CachedAsxCsvDataProvider(
        IAsxCsvDataProvider inner,
        IDistributedCache cache,
        ILogger<CachedAsxCsvDataProvider> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<AsxCompany>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var cachedBytes = await _cache.GetAsync(CacheKey, cancellationToken);
        if (cachedBytes != null)
        {
            try
            {
                var cachedCompanies = JsonSerializer.Deserialize<List<AsxCompany>>(cachedBytes);
                if (cachedCompanies != null && cachedCompanies.Count > 0)
                {
                    _logger.LogInformation("Cache hit: Retrieved {Count} companies from cache", cachedCompanies.Count);
                    return cachedCompanies;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize cached company list");
            }
        }

        _logger.LogInformation("Cache miss: Fetching company list from ASX");

        var companies = await _inner.GetAllCompaniesAsync(cancellationToken);
        var companiesList = companies.ToList();

        var serializedCompanies = JsonSerializer.SerializeToUtf8Bytes(companiesList);
        await _cache.SetAsync(CacheKey, serializedCompanies, CacheOptions, cancellationToken);

        _logger.LogInformation("Cached {Count} companies for {Duration}", companiesList.Count, CacheDuration);

        return companiesList;
    }
}
