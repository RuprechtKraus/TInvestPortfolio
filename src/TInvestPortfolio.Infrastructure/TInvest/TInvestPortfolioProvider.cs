using System.Collections.Concurrent;
using TInvestPortfolio.Core.Abstractions;
using TInvestPortfolio.Core.Models;
using TInvestPortfolio.Infrastructure.TInvest.Models;

namespace TInvestPortfolio.Infrastructure.TInvest;

/// <summary>
/// Загружает данные T-Invest и преобразует их в модели портфеля приложения.
/// </summary>
public sealed class TInvestPortfolioProvider(ITInvestClient client) : IPortfolioProvider
{
    /// <summary>
    /// Получает открытые инвестиционные счета пользователя.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список открытых инвестиционных счетов.</returns>
    public async Task<IReadOnlyList<InvestmentAccount>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AccountDto> accounts = await client.GetOpenAccountsAsync(cancellationToken);

        return accounts
            .Select(account => new InvestmentAccount(account.Id, account.Name))
            .ToList();
    }

    /// <summary>
    /// Получает и обогащает позиции выбранного инвестиционного счёта.
    /// </summary>
    /// <param name="account">Счёт, для которого запрашиваются позиции.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список позиций портфеля с данными эмитентов и облигаций.</returns>
    public async Task<IReadOnlyList<PortfolioPosition>> GetPositionsAsync(
        InvestmentAccount account,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PortfolioPositionDto> positions =
            await client.GetPortfolioAsync(account.Id, cancellationToken);

        var instrumentCache = new ConcurrentDictionary<string, Task<InstrumentDto>>();
        var assetCache = new ConcurrentDictionary<string, Task<AssetDto?>>();
        var bondCache = new ConcurrentDictionary<string, Task<BondDto?>>();

        IEnumerable<Task<PortfolioPosition>> positionTasks = positions
            .Where(position => !string.IsNullOrWhiteSpace(position.InstrumentUid))
            .Select(position => MapPositionAsync(
                        account,
                        position,
                        instrumentCache,
                        assetCache,
                        bondCache,
                        cancellationToken));

        return await Task.WhenAll(positionTasks);
    }

    private async Task<PortfolioPosition> MapPositionAsync(
        InvestmentAccount account,
        PortfolioPositionDto position,
        ConcurrentDictionary<string, Task<InstrumentDto>> instrumentCache,
        ConcurrentDictionary<string, Task<AssetDto?>> assetCache,
        ConcurrentDictionary<string, Task<BondDto?>> bondCache,
        CancellationToken cancellationToken)
    {
        var instrument =
            await instrumentCache.GetOrAdd(
                position.InstrumentUid,
                id => client.GetInstrumentAsync(id, cancellationToken));
        var asset = string.IsNullOrWhiteSpace(instrument.AssetUid)
            ? null
            : await assetCache.GetOrAdd(instrument.AssetUid, id => client.GetAssetAsync(id, cancellationToken));
        var bond = string.Equals(instrument.InstrumentType, "bond", StringComparison.OrdinalIgnoreCase)
            ? await bondCache.GetOrAdd(position.InstrumentUid, id => client.GetBondAsync(id, cancellationToken))
            : null;

        var quantity = position.Quantity?.ToDecimal() ?? 0m;
        var currentPrice = position.CurrentPrice?.ToDecimal() ?? 0m;
        var issuer = asset?.Brand?.Company ?? asset?.Brand?.Name ?? asset?.Name ?? instrument.Name;
        var sector = SectorLocalizer.ToRussian(asset?.Brand?.Sector);

        return new PortfolioPosition(
            account.Name,
            string.IsNullOrWhiteSpace(instrument.Ticker) ? position.Ticker : instrument.Ticker,
            instrument.Name,
            issuer,
            sector,
            instrument.InstrumentType,
            instrument.Isin,
            quantity,
            position.AveragePositionPrice?.ToDecimal() ?? 0m,
            currentPrice,
            quantity * currentPrice,
            position.ExpectedYield?.ToDecimal() ?? 0m,
            position.CurrentNkd?.ToDecimal() ?? 0m,
            position.CurrentPrice?.Currency ?? instrument.Currency,
            bond is null
                ? null
                : new BondDetails(
                    bond.Nominal?.ToDecimal(),
                    bond.MaturityDate is null ? null : DateOnly.FromDateTime(bond.MaturityDate.Value.DateTime),
                    bond.OfferDate is null ? null : DateOnly.FromDateTime(bond.OfferDate.Value.DateTime),
                    bond.FloatingCouponFlag,
                    bond.AmortizationFlag,
                    bond.SubordinatedFlag,
                    bond.RiskLevel));
    }

}
