using System.Globalization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace TInvestPortfolio.Infrastructure.TInvest.Models;

/// <summary>
/// Контракт ответа T-Invest API со списком брокерских счетов.
/// </summary>
[PublicAPI]
public sealed class AccountsResponse
{
    [JsonPropertyName("accounts")]
    public List<AccountDto> Accounts { get; init; } = [];
}

/// <summary>
/// Контракт брокерского счёта T-Invest API.
/// </summary>
[PublicAPI]
public sealed class AccountDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Контракт ответа T-Invest API с позициями портфеля.
/// </summary>
[PublicAPI]
public sealed class PortfolioResponse
{
    [JsonPropertyName("positions")]
    public List<PortfolioPositionDto> Positions { get; init; } = [];
}

/// <summary>
/// Контракт позиции портфеля T-Invest API.
/// </summary>
[PublicAPI]
public sealed class PortfolioPositionDto
{
    [JsonPropertyName("instrumentUid")]
    public string InstrumentUid { get; init; } = string.Empty;

    [JsonPropertyName("ticker")]
    public string Ticker { get; init; } = string.Empty;

    [JsonPropertyName("quantity")]
    public QuotationDto? Quantity { get; init; }

    [JsonPropertyName("averagePositionPrice")]
    public MoneyValueDto? AveragePositionPrice { get; init; }

    [JsonPropertyName("currentPrice")]
    public MoneyValueDto? CurrentPrice { get; init; }

    [JsonPropertyName("expectedYield")]
    public MoneyValueDto? ExpectedYield { get; init; }

    [JsonPropertyName("currentNkd")]
    public MoneyValueDto? CurrentNkd { get; init; }
}

/// <summary>
/// Контракт ответа T-Invest API со справочными данными инструмента.
/// </summary>
[PublicAPI]
public sealed class InstrumentResponse
{
    [JsonPropertyName("instrument")]
    public InstrumentDto Instrument { get; init; } = new InstrumentDto();
}

/// <summary>
/// Контракт справочных данных инструмента T-Invest API.
/// </summary>
[PublicAPI]
public sealed class InstrumentDto
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("isin")]
    public string? Isin { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("instrumentType")]
    public string InstrumentType { get; init; } = string.Empty;

    [JsonPropertyName("assetUid")]
    public string? AssetUid { get; init; }
}

/// <summary>
/// Контракт ответа T-Invest API с данными актива.
/// </summary>
[PublicAPI]
public sealed class AssetResponse
{
    [JsonPropertyName("asset")]
    public AssetDto Asset { get; init; } = new AssetDto();
}

/// <summary>
/// Контракт актива T-Invest API.
/// </summary>
[PublicAPI]
public sealed class AssetDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("brand")]
    public BrandDto? Brand { get; init; }
}

/// <summary>
/// Контракт бренда и эмитента T-Invest API.
/// </summary>
[PublicAPI]
public sealed class BrandDto
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("company")]
    public string? Company { get; init; }

    [JsonPropertyName("sector")]
    public string? Sector { get; init; }
}

/// <summary>
/// Контракт ответа T-Invest API с параметрами облигации.
/// </summary>
[PublicAPI]
public sealed class BondResponse
{
    [JsonPropertyName("instrument")]
    public BondDto Instrument { get; init; } = new BondDto();
}

/// <summary>
/// Контракт параметров облигации T-Invest API.
/// </summary>
[PublicAPI]
public sealed class BondDto
{
    [JsonPropertyName("nominal")]
    public MoneyValueDto? Nominal { get; init; }

    [JsonPropertyName("maturityDate")]
    public DateTimeOffset? MaturityDate { get; init; }

    [JsonPropertyName("offerDate")]
    public DateTimeOffset? OfferDate { get; init; }

    [JsonPropertyName("floatingCouponFlag")]
    public bool FloatingCouponFlag { get; init; }

    [JsonPropertyName("amortizationFlag")]
    public bool AmortizationFlag { get; init; }

    [JsonPropertyName("subordinatedFlag")]
    public bool SubordinatedFlag { get; init; }

    [JsonPropertyName("riskLevel")]
    public string? RiskLevel { get; init; }
}

/// <summary>
/// Контракт десятичной котировки T-Invest API.
/// </summary>
[PublicAPI]
public class QuotationDto
{
    [JsonPropertyName("units")]
    public string Units { get; init; } = "0";

    [JsonPropertyName("nano")]
    public int Nano { get; init; }

/// <summary>
/// Преобразует значение в денежный формат приложения.
/// </summary>
    public decimal ToDecimal()
    {
        return decimal.Parse(Units, CultureInfo.InvariantCulture) + Nano / 1_000_000_000m;
    }
}

/// <summary>
/// Контракт денежной котировки T-Invest API.
/// </summary>
[PublicAPI]
public sealed class MoneyValueDto : QuotationDto
{
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;
}
