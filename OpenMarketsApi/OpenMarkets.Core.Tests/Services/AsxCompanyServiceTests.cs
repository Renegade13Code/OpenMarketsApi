using Microsoft.Extensions.Logging;
using Moq;
using OpenMarkets.Core.DomainModels;
using OpenMarkets.Core.DomainModels.Exceptions;
using OpenMarkets.Core.Interfaces;
using OpenMarkets.Core.Services;
using Xunit;

namespace OpenMarkets.Core.Tests.Services;

public class AsxCompanyServiceTests
{
    private readonly Mock<IAsxCsvDataProvider> _mockCsvDataProvider;
    private readonly Mock<ILogger<AsxCompanyService>> _mockLogger;
    private readonly AsxCompanyService _sut;

    public AsxCompanyServiceTests()
    {
        _mockCsvDataProvider = new Mock<IAsxCsvDataProvider>();
        _mockLogger = new Mock<ILogger<AsxCompanyService>>();
        _sut = new AsxCompanyService(_mockCsvDataProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WithValidCode_ReturnsCompany()
    {
        var companies = new List<AsxCompany>
        {
            new() { AsxCode = "CBA", CompanyName = "Commonwealth Bank", GicsIndustry = "Banks" },
            new() { AsxCode = "BHP", CompanyName = "BHP Group", GicsIndustry = "Materials" }
        };

        _mockCsvDataProvider
            .Setup(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        var result = await _sut.GetCompanyByCodeAsync("CBA");

        Assert.NotNull(result);
        Assert.Equal("CBA", result.AsxCode);
        Assert.Equal("Commonwealth Bank", result.CompanyName);
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WithCaseInsensitiveCode_ReturnsCompany()
    {
        var companies = new List<AsxCompany>
        {
            new() { AsxCode = "CBA", CompanyName = "Commonwealth Bank", GicsIndustry = "Banks" }
        };

        _mockCsvDataProvider
            .Setup(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        var result = await _sut.GetCompanyByCodeAsync("cba");

        Assert.NotNull(result);
        Assert.Equal("CBA", result.AsxCode);
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WithInvalidCode_ThrowsCompanyNotFoundException()
    {
        var companies = new List<AsxCompany>
        {
            new() { AsxCode = "CBA", CompanyName = "Commonwealth Bank", GicsIndustry = "Banks" }
        };

        _mockCsvDataProvider
            .Setup(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(companies);

        await Assert.ThrowsAsync<CompanyNotFoundException>(
            () => _sut.GetCompanyByCodeAsync("INVALID"));
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WithNullCode_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetCompanyByCodeAsync(null!));
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WithEmptyCode_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.GetCompanyByCodeAsync(string.Empty));
    }

    [Fact]
    public async Task GetCompanyByCodeAsync_WhenDataProviderThrows_ThrowsAsxDataUnavailableException()
    {
        _mockCsvDataProvider
            .Setup(x => x.GetAllCompaniesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("CSV fetch failed"));

        await Assert.ThrowsAsync<AsxDataUnavailableException>(
            () => _sut.GetCompanyByCodeAsync("CBA"));
    }
}
