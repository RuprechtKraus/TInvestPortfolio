using TInvestPortfolio.Infrastructure.TInvest.Models;

namespace TInvestPortfolio.Infrastructure.TInvest;

/// <summary>
/// Выполняет запросы к REST API T-Invest.
/// </summary>
public interface ITInvestClient
{
    /// <summary>
    /// Возвращает открытые счета T-Invest.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Список контрактов открытых счетов.</returns>
    Task<IReadOnlyList<AccountDto>> GetOpenAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает позиции портфеля указанного счёта.
    /// </summary>
    /// <param name="accountId">Идентификатор инвестиционного счёта T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Список контрактов позиций портфеля.</returns>
    Task<IReadOnlyList<PortfolioPositionDto>> GetPortfolioAsync(
        string accountId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает справочную информацию об инструменте.
    /// </summary>
    /// <param name="instrumentUid">Уникальный идентификатор инструмента T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Контракт справочных данных инструмента.</returns>
    Task<InstrumentDto> GetInstrumentAsync(string instrumentUid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает подробную информацию об активе.
    /// </summary>
    /// <param name="assetUid">Уникальный идентификатор актива T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Контракт актива или <see langword="null"/>, если API не вернул данные.</returns>
    Task<AssetDto?> GetAssetAsync(string assetUid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает параметры облигации.
    /// </summary>
    /// <param name="instrumentUid">Уникальный идентификатор облигации T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Контракт параметров облигации или <see langword="null"/>, если API не вернул данные.</returns>
    Task<BondDto?> GetBondAsync(string instrumentUid, CancellationToken cancellationToken = default);
}
