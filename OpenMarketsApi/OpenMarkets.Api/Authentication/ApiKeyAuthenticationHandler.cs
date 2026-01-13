using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OpenMarkets.Api.Configuration;

namespace OpenMarkets.Api.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private readonly ApiKeyConfiguration _apiKeyConfiguration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<ApiKeyConfiguration> apiKeyConfiguration)
        : base(options, logger, encoder)
    {
        _apiKeyConfiguration = apiKeyConfiguration.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key header not found"));
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is empty"));
        }

        var apiKeyEntry = _apiKeyConfiguration.Keys.FirstOrDefault(k => k.Key == providedApiKey);
        if (apiKeyEntry == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, apiKeyEntry.ClientName),
            new Claim("ClientName", apiKeyEntry.ClientName)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        Logger.LogInformation("API Key authentication successful for client: {ClientName}", apiKeyEntry.ClientName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
