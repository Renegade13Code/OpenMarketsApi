using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMarkets.Core.DomainModels;
using OpenMarkets.Core.Interfaces;
using OpenMarkets.Infrastructure.Csv;
using System.Text.Json;
using Xunit;

namespace OpenMarkets.Infrastructure.Tests.Csv;

public class CachedAsxCsvDataProviderTests
{
    private readonly Mock<IAsxCsvDataProvider> _mockInnerProvider;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<CachedAsxCsvDataProvider>> _mockLogger;
    private readonly CachedAsxCsvDataProvider _sut;

    public CachedAsxCsvDataProviderTests()
    {
        _mockInnerProvider = new Mock<IAsxCsvDataProvider>();
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<CachedAsxCsvDataProvider>>();
        _sut = new CachedAsxCsvDataProvider(_mockInnerProvider.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllCompaniesAsync_WhenCached_ReturnsCachedList()
    {
        var cachedCompanies = new List<AsxCompany>
        {
            new() { AsxCode = "CBA", CompanyName = "Commonwealth Bank", GicsIndustry = "Banks" },
            new() { AsxCode = "BHP", CompanyName = "BHP Group", GicsIndustry = "Materials" }
        };

        var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(cachedCompanies);

        _mockCache
            .Setup(x => x.GetAsync("AsxCompanies_All", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _sut.GetAllCompaniesAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockInnerProvider.Verify(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllCompaniesAsync_WhenNotCached_FetchesAndCachesData()
    {
        var companies = new List<AsxCompany>
        {
            new() { AsxCode = "CBA", CompanyName = "Commonwealth Bank", GicsIndustry = "Banks" },
            new() { AsxCode = "BHP", CompanyName = "BHP Group", GicsIndustry = "Materials" }
        };

        _mockCache
            .Setup(x => x.GetAsync("AsxCompanies_All", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _mockInnerProvider
            .Setup(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        var result = await _sut.GetAllCompaniesAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        _mockInnerProvider.Verify(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetAsync(
            "AsxCompanies_All",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCompaniesAsync_WhenCacheDeserializationFails_FetchesFromSource()
    {
        var invalidBytes = new byte[] { 0xFF, 0xFE };

        _mockCache
            .Setup(x => x.GetAsync("AsxCompanies_All", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidBytes);

        var companies = new List<AsxCompany>
        {
            new() { AsxCode = "CBA", CompanyName = "Commonwealth Bank", GicsIndustry = "Banks" }
        };

        _mockInnerProvider
            .Setup(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        var result = await _sut.GetAllCompaniesAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        _mockInnerProvider.Verify(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
