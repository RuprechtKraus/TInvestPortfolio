namespace TInvestPortfolio.Core.Models;

/// <summary>
/// Представляет снимок портфеля и агрегированные данные по эмитентам.
/// </summary>
public sealed record PortfolioSnapshot(
    IReadOnlyList<PortfolioPosition> Positions,
    IReadOnlyList<IssuerPosition> Issuers);
