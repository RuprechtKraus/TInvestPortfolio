using JetBrains.Annotations;

namespace TInvestPortfolio.Cli.Configuration;

/// <summary>
/// Представляет настройки сохранения экспортируемого отчёта.
/// </summary>
[PublicAPI]
public sealed class ExportOptions
{
    public const string SectionName = "Export";
    public string OutputDirectory { get; init; } = "Reports";
}
