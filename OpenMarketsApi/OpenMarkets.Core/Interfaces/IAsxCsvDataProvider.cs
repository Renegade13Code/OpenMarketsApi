using OpenMarkets.Core.DomainModels;

namespace OpenMarkets.Core.Interfaces;

/// <summary>
/// Provides access to ASX company data from CSV source.
/// </summary>
public interface IAsxCsvDataProvider
{
    /// <summary>
    /// Retrieves all ASX-listed companies from the CSV source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all ASX companies</returns>
    Task<IEnumerable<AsxCompany>> GetAllCompaniesAsync(CancellationToken cancellationToken = default);
}
