using Microsoft.Extensions.DependencyInjection;
using OpenMarkets.Core.Services;

namespace OpenMarkets.Core;

public static class Bootstrapper
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddScoped<IAsxCompanyService, AsxCompanyService>();

        return services;
    }
}