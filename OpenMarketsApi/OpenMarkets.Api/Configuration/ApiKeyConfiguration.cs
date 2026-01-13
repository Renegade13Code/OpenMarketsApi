namespace OpenMarkets.Api.Configuration;

public class ApiKeyConfiguration
{
    public const string SectionName = "ApiKeys";

    public List<ApiKeyEntry> Keys { get; set; } = new();
}

public class ApiKeyEntry
{
    public string Key { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
}
