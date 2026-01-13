using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMarkets.Core.DomainModels;
using OpenMarkets.Core.Services;
using System.Text;
using System.Text.Json;
using Xunit;

namespace OpenMarkets.Core.Tests.Services;

public class CachedAsxCompanyServiceTests
{
    private readonly Mock<IAsxCompanyService> _mockInnerService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<CachedAsxCompanyService>> _mockLogger;
    private readonly CachedAsxCompanyService _sut;

    public CachedAsxCompanyServiceTests()
    {
        _mockInnerService = new Mock<IAsxCompanyService>();
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<CachedAsxCompanyService>>();
        _sut = new CachedAsxCompanyService(_mockInnerService.Object, _mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WhenCached_ReturnsCachedValue()
    {
        var cachedCompany = new AsxCompany
        {
            AsxCode = "CBA",
            CompanyName = "Commonwealth Bank",
            GicsIndustry = "Banks"
        };

        var cachedBytes = JsonSerializer.SerializeToUtf8Bytes(cachedCompany);

        _mockCache
            .Setup(x => x.GetAsync("AsxCompany_CBA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await _sut.GetCompanyByCodeAsync("CBA");

        Assert.NotNull(result);
        Assert.Equal("CBA", result.AsxCode);
        _mockInnerService.Verify(x => x.GetCompanyByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WhenNotCached_CallsInnerServiceAndCaches()
    {
        var company = new AsxCompany
        {
            AsxCode = "CBA",
            CompanyName = "Commonwealth Bank",
            GicsIndustry = "Banks"
        };

        _mockCache
            .Setup(x => x.GetAsync("AsxCompany_CBA", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _mockInnerService
            .Setup(x => x.GetCompanyByCodeAsync("CBA", It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var result = await _sut.GetCompanyByCodeAsync("CBA");

        Assert.NotNull(result);
        Assert.Equal("CBA", result.AsxCode);

        _mockInnerService.Verify(x => x.GetCompanyByCodeAsync("CBA", It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetAsync(
            "AsxCompany_CBA",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WithLowercaseCode_UsesCacheKeyWithUppercase()
    {
        var company = new AsxCompany
        {
            AsxCode = "CBA",
            CompanyName = "Commonwealth Bank",
            GicsIndustry = "Banks"
        };

        _mockCache
            .Setup(x => x.GetAsync("AsxCompany_CBA", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _mockInnerService
            .Setup(x => x.GetCompanyByCodeAsync("cba", It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        await _sut.GetCompanyByCodeAsync("cba");

        _mockCache.Verify(x => x.GetAsync("AsxCompany_CBA", It.IsAny<CancellationToken>()), Times.Once);
    }
}
