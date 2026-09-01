using NUnit.Framework;
using TInvestPortfolio.Core.Abstractions;
using TInvestPortfolio.Core.Models;
using TInvestPortfolio.Core.Services;

namespace TInvestPortfolio.Core.Tests;

[TestFixture]
public sealed class PortfolioExportServiceTests
{
    [Test]
    public async Task ExportAsync_SelectedAccount_ExportsProviderPositionsAndPassesSnapshotToExporter()
    {
        // Arrange
        PortfolioPosition[] positions = new[] { Position("Issuer A", 100m), Position("Issuer B", 50m) };
        var provider = new StubPortfolioProvider(positions);
        var exporter = new CapturingPortfolioExporter();
        var service = new PortfolioExportService(provider, exporter, new IssuerAggregator());
        var account = new InvestmentAccount("account-id", "Account");

        // Act
        var result = await service.ExportAsync(account, "portfolio.xlsx");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(provider.CallCount, Is.EqualTo(1));
            Assert.That(provider.SelectedAccount, Is.SameAs(account));
            Assert.That(exporter.Path, Is.EqualTo("portfolio.xlsx"));
            Assert.That(exporter.Snapshot, Is.SameAs(result));
            Assert.That(result.Issuers.Single(item => item.Issuer == "Issuer A").Share, Is.EqualTo(2m / 3m));
        });
    }

    private static PortfolioPosition Position(string issuer, decimal marketValue)
    {
        return new PortfolioPosition(
            "Account",
            "TICKER",
            "Instrument",
            issuer,
            "Не указан",
            "bond",
            null,
            1m,
            marketValue,
            marketValue,
            marketValue,
            0m,
            0m,
            "RUB",
            null);
    }

    private sealed class StubPortfolioProvider(IReadOnlyList<PortfolioPosition> positions) : IPortfolioProvider
    {
        public int CallCount { get; private set; }
        public InvestmentAccount? SelectedAccount { get; private set; }

        public Task<IReadOnlyList<InvestmentAccount>> GetAccountsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<InvestmentAccount>>([]);

        public Task<IReadOnlyList<PortfolioPosition>> GetPositionsAsync(
            InvestmentAccount account,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            SelectedAccount = account;

            return Task.FromResult(positions);
        }
    }

    private sealed class CapturingPortfolioExporter : IPortfolioExporter
    {
        public PortfolioSnapshot? Snapshot { get; private set; }
        public string? Path { get; private set; }

        public Task ExportAsync(PortfolioSnapshot snapshot, string path, CancellationToken cancellationToken = default)
        {
            Snapshot = snapshot;
            Path = path;

            return Task.CompletedTask;
        }
    }
}
