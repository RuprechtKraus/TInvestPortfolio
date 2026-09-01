using TInvestPortfolio.Core.Models;

namespace TInvestPortfolio.Core.Abstractions;

/// <summary>
/// Сохраняет снимок портфеля в заданном формате.
/// </summary>
public interface IPortfolioExporter
{
    /// <summary>
    /// Экспортирует снимок портфеля в файл по указанному пути.
    /// </summary>
    /// <param name="snapshot">Снимок портфеля для сохранения.</param>
    /// <param name="path">Путь к создаваемому файлу.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, завершающаяся после сохранения файла.</returns>
    Task ExportAsync(PortfolioSnapshot snapshot, string path, CancellationToken cancellationToken = default);
}
