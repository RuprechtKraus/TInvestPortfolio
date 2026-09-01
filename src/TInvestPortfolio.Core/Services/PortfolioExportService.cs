using TInvestPortfolio.Core.Abstractions;
using TInvestPortfolio.Core.Models;

namespace TInvestPortfolio.Core.Services;

/// <summary>
/// Получает позиции выбранного счёта, формирует снимок портфеля и передаёт его экспортёру.
/// </summary>
public sealed class PortfolioExportService(
    IPortfolioProvider portfolioProvider,
    IPortfolioExporter exporter,
    IssuerAggregator issuerAggregator)
{
    /// <summary>
    /// Экспортирует текущий портфель указанного инвестиционного счёта в файл.
    /// </summary>
    /// <param name="account">Счёт, позиции которого необходимо экспортировать.</param>
    /// <param name="path">Путь к создаваемому файлу экспорта.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Сформированный снимок портфеля.</returns>
    public async Task<PortfolioSnapshot> ExportAsync(
        InvestmentAccount account,
        string path,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PortfolioPosition> positions = await portfolioProvider.GetPositionsAsync(account, cancellationToken);
        var snapshot = issuerAggregator.CreateSnapshot(positions);
        await exporter.ExportAsync(snapshot, path, cancellationToken);

        return snapshot;
    }
}
