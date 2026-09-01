using JetBrains.Annotations;

namespace TInvestPortfolio.Infrastructure.Configuration;

/// <summary>
/// Представляет настройки подключения к T-Invest API.
/// </summary>
[PublicAPI]
public sealed class TInvestOptions
{
    public const string SectionName = "TInvest";

    public string Token { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://invest-public-api.tbank.ru";
}
