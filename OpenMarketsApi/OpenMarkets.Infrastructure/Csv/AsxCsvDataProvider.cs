using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using OpenMarkets.Core.DomainModels;
using OpenMarkets.Core.Interfaces;

namespace OpenMarkets.Infrastructure.Csv;

public class AsxCsvDataProvider : IAsxCsvDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AsxCsvDataProvider> _logger;
    private const string AsxCsvUrl = "https://www.asx.com.au/asx/research/ASXListedCompanies.csv";

    public AsxCsvDataProvider(HttpClient httpClient, ILogger<AsxCsvDataProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<AsxCompany>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching ASX company data from {Url}", AsxCsvUrl);

            var response = await _httpClient.GetAsync(AsxCsvUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            await reader.ReadLineAsync(cancellationToken);
            await reader.ReadLineAsync(cancellationToken);

            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null, // Ignore missing fields
                BadDataFound = null, // Ignore bad data
                TrimOptions = TrimOptions.Trim
            });

            // Register the mapping
            csv.Context.RegisterClassMap<AsxCompanyCsvMap>();

            // Read all records into memory
            var companies = csv.GetRecords<AsxCompany>().ToList();

            _logger.LogInformation("Successfully fetched and parsed {Count} ASX companies", companies.Count);

            return companies;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch ASX company data from {Url}", AsxCsvUrl);
            throw new InvalidOperationException("Unable to fetch ASX company data. Please try again later.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Request to fetch ASX company data was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse ASX company data");
            throw new InvalidOperationException("Unable to parse ASX company data.", ex);
        }
    }
}
