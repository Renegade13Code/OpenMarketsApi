namespace OpenMarkets.Core.DomainModels;

public class AsxCompany
{
    public string AsxCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? GicsIndustry { get; set; }
}
