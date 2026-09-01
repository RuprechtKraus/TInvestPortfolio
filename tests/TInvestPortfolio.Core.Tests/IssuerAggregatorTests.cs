using NUnit.Framework;
using TInvestPortfolio.Core.Models;
using TInvestPortfolio.Core.Services;

namespace TInvestPortfolio.Core.Tests;

[TestFixture]
public sealed class IssuerAggregatorTests
{
    private readonly IssuerAggregator _aggregator = new IssuerAggregator();

    [Test]
    public void CreateSnapshot_MultiplePositionsForSameIssuer_AggregatesMarketValueAndShare()
    {
        // Arrange
        PortfolioPosition[] positions =
        [
            Position("Issuer A", "RUB", 100m),
            Position("Issuer A", "RUB", 50m),
            Position("Issuer B", "RUB", 50m)
        ];

        // Act
        var snapshot = _aggregator.CreateSnapshot(positions);

        // Assert
        Assert.That(snapshot.Issuers, Has.Count.EqualTo(2));
        var issuerA = snapshot.Issuers.Single(item => item.Issuer == "Issuer A");
        Assert.Multiple(() =>
        {
            Assert.That(issuerA.MarketValue, Is.EqualTo(150m));
            Assert.That(issuerA.Share, Is.EqualTo(0.75m));
        });
    }

    [Test]
    public void CreateSnapshot_PositionsInDifferentCurrencies_CalculatesSharesPerCurrency()
    {
        // Arrange
        PortfolioPosition[] positions =
        [
            Position("Issuer A", "RUB", 100m),
            Position("Issuer B", "RUB", 100m),
            Position("Issuer A", "USD", 20m)
        ];

        // Act
        var snapshot = _aggregator.CreateSnapshot(positions);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(
                snapshot.Issuers.Single(item => item.Issuer == "Issuer A" && item.Currency == "RUB").Share,
                Is.EqualTo(0.5m));
            Assert.That(
                snapshot.Issuers.Single(item => item.Issuer == "Issuer B" && item.Currency == "RUB").Share,
                Is.EqualTo(0.5m));
            Assert.That(
                snapshot.Issuers.Single(item => item.Issuer == "Issuer A" && item.Currency == "USD").Share,
                Is.EqualTo(1m));
        });
    }

    [Test]
    public void CreateSnapshot_EmptyPortfolio_ReturnsEmptyPositionsAndIssuers()
    {
        // Arrange
        var positions = Array.Empty<PortfolioPosition>();

        // Act
        var snapshot = _aggregator.CreateSnapshot(positions);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(snapshot.Positions, Is.Empty);
            Assert.That(snapshot.Issuers, Is.Empty);
        });
    }

    [Test]
    public void CreateSnapshot_ZeroCurrencyTotal_ReturnsZeroShare()
    {
        // Arrange
        var positions = new[] { Position("Issuer A", "RUB", 0m) };

        // Act
        var snapshot = _aggregator.CreateSnapshot(positions);

        // Assert
        Assert.That(snapshot.Issuers.Single().Share, Is.EqualTo(0m));
    }

    [Test]
    public void CreateSnapshot_MultipleSectorsForIssuer_UsesFirstNonEmptySector()
    {
        // Arrange
        PortfolioPosition[] positions =
        [
            Position("Issuer A", "RUB", 100m, ""),
            Position("Issuer A", "RUB", 50m, "Финансы")
        ];

        // Act
        var snapshot = _aggregator.CreateSnapshot(positions);

        // Assert
        Assert.That(snapshot.Issuers.Single().Sector, Is.EqualTo("Финансы"));
    }

    [Test]
    public void CreateSnapshot_EmptySector_ReturnsNotSpecifiedSector()
    {
        // Arrange
        var positions = new[] { Position("Issuer A", "RUB", 100m, "") };

        // Act
        var snapshot = _aggregator.CreateSnapshot(positions);

        // Assert
        Assert.That(snapshot.Issuers.Single().Sector, Is.EqualTo("Не указан"));
    }

    private static PortfolioPosition Position(string issuer, string currency, decimal marketValue, string sector = "Не указан")
    {
        return new PortfolioPosition(
            "Account",
            "TICKER",
            "Instrument",
            issuer,
            sector,
            "bond",
            null,
            1m,
            marketValue,
            marketValue,
            marketValue,
            0m,
            0m,
            currency,
            null);
    }
}
