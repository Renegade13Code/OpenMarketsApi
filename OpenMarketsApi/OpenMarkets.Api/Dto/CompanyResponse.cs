namespace OpenMarkets.Api.Dto;

public class CompanyResponse
{
    public string AsxCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? GicsIndustry { get; set; }
}
