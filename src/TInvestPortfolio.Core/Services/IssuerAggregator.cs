using TInvestPortfolio.Core.Models;

namespace TInvestPortfolio.Core.Services;

/// <summary>
/// Формирует снимок портфеля и агрегирует позиции по эмитентам и валютам.
/// </summary>
public sealed class IssuerAggregator
{
    /// <summary>
    /// Создаёт снимок портфеля с рассчитанной стоимостью и долей каждого эмитента в своей валюте.
    /// </summary>
    /// <param name="positions">Позиции портфеля для агрегации.</param>
    /// <returns>Снимок портфеля с исходными позициями и сводкой по эмитентам.</returns>
    public PortfolioSnapshot CreateSnapshot(IEnumerable<PortfolioPosition> positions)
    {
        List<PortfolioPosition> materializedPositions = positions
            .OrderBy(
                position => position.Issuer,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                position => position.Name,
                StringComparer.OrdinalIgnoreCase)
            .ToList();

        var issuerTotals = materializedPositions
            .GroupBy(position => new { position.Issuer, position.Currency })
            .Select(group => new
            {
                group.Key.Issuer,
                group.Key.Currency,
                Sector = group
                    .Select(position => position.Sector)
                    .FirstOrDefault(sector => !string.IsNullOrWhiteSpace(sector)) ?? "Не указан",
                MarketValue = group.Sum(position => position.MarketValue)
            })
            .ToList();

        List<IssuerPosition> issuers = issuerTotals
            .Select(total =>
            {
                var currencyTotal = issuerTotals
                    .Where(item => string.Equals(
                               item.Currency,
                               total.Currency,
                               StringComparison.OrdinalIgnoreCase))
                    .Sum(item => item.MarketValue);

                return new IssuerPosition(
                    total.Issuer,
                    total.Sector,
                    total.Currency,
                    total.MarketValue,
                    currencyTotal == 0m ? 0m : total.MarketValue / currencyTotal);
            })
            .OrderByDescending(position => position.MarketValue)
            .ThenBy(position => position.Issuer, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new PortfolioSnapshot(materializedPositions, issuers);
    }
}
