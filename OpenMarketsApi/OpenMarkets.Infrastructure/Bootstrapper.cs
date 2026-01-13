using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMarkets.Core.Interfaces;
using OpenMarkets.Infrastructure.Csv;

namespace OpenMarkets.Infrastructure;

public static class Bootstrapper
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IAsxCsvDataProvider, AsxCsvDataProvider>();
        services.Decorate<IAsxCsvDataProvider, CachedAsxCsvDataProvider>();

        return services;
    }
}
