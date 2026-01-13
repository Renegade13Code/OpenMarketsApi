using OpenMarkets.Core.DomainModels;

namespace OpenMarkets.Core.Services;

public interface IAsxCompanyService
{
    /// <summary>
    /// Retrieves an ASX-listed company by its ASX code.
    /// </summary>
    /// <param name="asxCode">The ASX code (e.g., "CBA", "BHP")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The ASX company information</returns>
    /// <exception cref="DomainModels.Exceptions.CompanyNotFoundException">Thrown when the ASX code is not found</exception>
    /// <exception cref="DomainModels.Exceptions.AsxDataUnavailableException">Thrown when ASX data cannot be retrieved</exception>
    Task<AsxCompany> GetCompanyByCodeAsync(string asxCode, CancellationToken cancellationToken = default);
}
