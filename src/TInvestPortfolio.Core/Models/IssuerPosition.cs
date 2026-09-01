namespace TInvestPortfolio.Core.Models;

/// <summary>
/// Представляет агрегированную рыночную стоимость позиций одного эмитента.
/// </summary>
public sealed record IssuerPosition(string Issuer, string Sector, string Currency, decimal MarketValue, decimal Share);
