using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TInvestPortfolio.Core.Abstractions;
using TInvestPortfolio.Infrastructure.Configuration;
using TInvestPortfolio.Infrastructure.Excel;
using TInvestPortfolio.Infrastructure.TInvest;

namespace TInvestPortfolio.Infrastructure.DependencyInjection;

/// <summary>
/// Содержит методы регистрации инфраструктурных зависимостей в DI-контейнере.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует клиент T-Invest, провайдер портфеля и Excel-экспортёр.
    /// </summary>
    /// <param name="services">Коллекция сервисов, в которую добавляются регистрации.</param>
    /// <returns>Та же коллекция сервисов с добавленными инфраструктурными зависимостями.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TInvestOptions>()
            .Bind(configuration.GetSection(TInvestOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "TInvest:BaseUrl must be an absolute URL.");

        services.AddHttpClient<ITInvestClient, TInvestClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<TInvestOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<IPortfolioProvider, TInvestPortfolioProvider>();
        services.AddSingleton<IPortfolioExporter, ExcelPortfolioExporter>();

        return services;
    }
}
