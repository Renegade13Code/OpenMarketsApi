using Microsoft.Extensions.Logging;
using OpenMarkets.Core.DomainModels;
using OpenMarkets.Core.DomainModels.Exceptions;
using OpenMarkets.Core.Interfaces;

namespace OpenMarkets.Core.Services;

public class AsxCompanyService : IAsxCompanyService
{
    private readonly IAsxCsvDataProvider _csvDataProvider;
    private readonly ILogger<AsxCompanyService> _logger;

    public AsxCompanyService(
        IAsxCsvDataProvider csvDataProvider,
        ILogger<AsxCompanyService> logger)
    {
        _csvDataProvider = csvDataProvider;
        _logger = logger;
    }

    public async Task<AsxCompany> GetCompanyByCodeAsync(string asxCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(asxCode))
        {
            throw new ArgumentException("ASX code cannot be null or empty.", nameof(asxCode));
        }

        IEnumerable<AsxCompany> companies;

        try
        {
            companies = await _csvDataProvider.GetAllCompaniesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ASX company data");
            throw new AsxDataUnavailableException("Unable to retrieve ASX company data.", ex);
        }

        // Case-insensitive lookup
        var company = companies.FirstOrDefault(c =>
            c.AsxCode.Equals(asxCode, StringComparison.OrdinalIgnoreCase));

        if (company == null)
        {
            _logger.LogWarning("Company with ASX code '{AsxCode}' not found", asxCode);
            throw new CompanyNotFoundException(asxCode);
        }

        _logger.LogInformation("Successfully retrieved company with ASX code '{AsxCode}'", asxCode);
        return company;
    }
}
