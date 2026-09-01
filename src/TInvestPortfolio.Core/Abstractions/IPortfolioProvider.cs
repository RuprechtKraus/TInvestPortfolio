using TInvestPortfolio.Core.Models;

namespace TInvestPortfolio.Core.Abstractions;

/// <summary>
/// Предоставляет данные инвестиционных счетов и их портфельных позиций.
/// </summary>
public interface IPortfolioProvider
{
    /// <summary>
    /// Возвращает открытые инвестиционные счета.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список открытых инвестиционных счетов.</returns>
    Task<IReadOnlyList<InvestmentAccount>> GetAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает позиции выбранного инвестиционного счёта.
    /// </summary>
    /// <param name="account">Счёт, позиции которого необходимо получить.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список позиций выбранного счёта.</returns>
    Task<IReadOnlyList<PortfolioPosition>> GetPositionsAsync(
        InvestmentAccount account,
        CancellationToken cancellationToken = default);
}
