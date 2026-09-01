namespace TInvestPortfolio.Core.Models;

/// <summary>
/// Представляет позицию портфеля в независимом от API формате приложения.
/// </summary>
public sealed record PortfolioPosition(
    string AccountName,
    string Ticker,
    string Name,
    string Issuer,
    string Sector,
    string InstrumentType,
    string? Isin,
    decimal Quantity,
    decimal AveragePrice,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal ExpectedYield,
    decimal CurrentNkd,
    string Currency,
    BondDetails? Bond);

/// <summary>
/// Представляет специфичные для облигации параметры позиции.
/// </summary>
public sealed record BondDetails(
    decimal? Nominal,
    DateOnly? MaturityDate,
    DateOnly? OfferDate,
    bool FloatingCoupon,
    bool Amortizing,
    bool Subordinated,
    string? RiskLevel);
