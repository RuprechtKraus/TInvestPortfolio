using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text;
using TInvestPortfolio.Cli.Configuration;
using TInvestPortfolio.Core.Abstractions;
using TInvestPortfolio.Core.Models;
using TInvestPortfolio.Core.Services;
using TInvestPortfolio.Infrastructure.Configuration;
using TInvestPortfolio.Infrastructure.DependencyInjection;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, false)
    .AddUserSecrets<Program>(true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.Configure<ExportOptions>(builder.Configuration.GetSection(ExportOptions.SectionName));
builder.Services.AddSingleton<IssuerAggregator>();
builder.Services.AddTransient<PortfolioExportService>();
builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();
var tInvestOptions = host.Services.GetRequiredService<IOptions<TInvestOptions>>().Value;
if (string.IsNullOrWhiteSpace(tInvestOptions.Token))
{
    Console.Error.WriteLine("T-Invest token is not configured. Use User Secrets (TInvest:Token) or TInvest__Token.");

    return 2;
}

var exportOptions = host.Services.GetRequiredService<IOptions<ExportOptions>>().Value;
var outputPath = args.FirstOrDefault(argument => !argument.StartsWith('-')) ??
                 Path.Combine(exportOptions.OutputDirectory, $"portfolio-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");
outputPath = Path.GetFullPath(outputPath);

try
{
    IPortfolioProvider portfolioProvider = host.Services.GetRequiredService<IPortfolioProvider>();
    IReadOnlyList<InvestmentAccount> accounts = await portfolioProvider.GetAccountsAsync();
    InvestmentAccount? account = SelectAccount(accounts);
    if (account is null)
    {
        Console.WriteLine("Export cancelled.");

        return 0;
    }

    Console.WriteLine($"Loading portfolio for account: {account.Name}");
    var exportService = host.Services.GetRequiredService<PortfolioExportService>();
    var snapshot = await exportService.ExportAsync(account, outputPath);
    Console.WriteLine($"Exported {snapshot.Positions.Count} positions to:");
    Console.WriteLine(outputPath);

    return 0;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine($"T-Invest API request failed: {exception.Message}");

    return 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(exception.Message);

    return 1;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");

    return 1;
}

static InvestmentAccount? SelectAccount(IReadOnlyList<InvestmentAccount> accounts)
{
    if (accounts.Count == 0)
    {
        throw new InvalidOperationException("No open T-Invest accounts were found.");
    }

    Console.WriteLine("Available accounts:");
    for (var index = 0; index < accounts.Count; index++)
    {
        Console.WriteLine($"[{index + 1}] {accounts[index].Name} ({accounts[index].Id})");
    }

    while (true)
    {
        Console.Write($"Select an account [1-{accounts.Count}] or q to exit: ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input) ||
            string.Equals(input, "q", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(input, "й", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (int.TryParse(input, out var selected) && selected >= 1 && selected <= accounts.Count)
        {
            return accounts[selected - 1];
        }

        Console.WriteLine("Enter a number from the displayed list, or q to exit.");
    }
}
