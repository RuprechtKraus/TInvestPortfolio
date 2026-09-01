using ClosedXML.Excel;
using TInvestPortfolio.Core.Abstractions;
using TInvestPortfolio.Core.Models;

namespace TInvestPortfolio.Infrastructure.Excel;

/// <summary>
/// Сохраняет снимок портфеля в Excel-файл формата XLSX.
/// </summary>
public sealed class ExcelPortfolioExporter : IPortfolioExporter
{
    /// <summary>
    /// Создаёт Excel-отчёт с листами позиций и сводки по эмитентам.
    /// </summary>
    /// <param name="snapshot">Снимок портфеля для экспорта.</param>
    /// <param name="path">Путь к создаваемому XLSX-файлу.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Задача, завершающаяся после сохранения файла.</returns>
    public Task ExportAsync(PortfolioSnapshot snapshot, string path, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        using var workbook = new XLWorkbook();
        CreatePortfolioSheet(workbook, snapshot);
        CreateIssuerSheet(workbook, snapshot);
        workbook.SaveAs(path);

        return Task.CompletedTask;
    }

    private static void CreatePortfolioSheet(XLWorkbook workbook, PortfolioSnapshot snapshot)
    {
        var sheet = workbook.Worksheets.Add("Портфель");
        var headers = new[]
        {
            "Счёт", "Эмитент", "Тикер", "Инструмент", "ISIN", "Тип", "Количество",
            "Средняя цена", "Текущая цена", "Стоимость", "Доход/убыток", "НКД", "Валюта",
            "Номинал", "Погашение", "Оферта", "Плавающий купон", "Амортизация", "Суборд.", "Риск"
        };
        WriteHeader(sheet, headers);

        var row = 2;
        foreach (var position in snapshot.Positions)
        {
            sheet.Cell(row, 1).Value = position.AccountName;
            sheet.Cell(row, 2).Value = position.Issuer;
            sheet.Cell(row, 3).Value = position.Ticker;
            sheet.Cell(row, 4).Value = position.Name;
            sheet.Cell(row, 5).Value = position.Isin;
            sheet.Cell(row, 6).Value = position.InstrumentType;
            sheet.Cell(row, 7).Value = position.Quantity;
            sheet.Cell(row, 8).Value = position.AveragePrice;
            sheet.Cell(row, 9).Value = position.CurrentPrice;
            sheet.Cell(row, 10).Value = position.MarketValue;
            sheet.Cell(row, 11).Value = position.ExpectedYield;
            sheet.Cell(row, 12).Value = position.CurrentNkd;
            sheet.Cell(row, 13).Value = position.Currency;
            sheet.Cell(row, 14).Value = position.Bond?.Nominal;
            sheet.Cell(row, 15).Value = position.Bond?.MaturityDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            sheet.Cell(row, 16).Value = position.Bond?.OfferDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            sheet.Cell(row, 17).Value = position.Bond is null ? string.Empty : ToYesNo(position.Bond.FloatingCoupon);
            sheet.Cell(row, 18).Value = position.Bond is null ? string.Empty : ToYesNo(position.Bond.Amortizing);
            sheet.Cell(row, 19).Value = position.Bond is null ? string.Empty : ToYesNo(position.Bond.Subordinated);
            sheet.Cell(row, 20).Value = FormatRiskLevel(position.Bond?.RiskLevel);
            row++;
        }

        FormatTable(sheet, row - 1, headers.Length, [7, 8, 9, 10, 11, 12, 14]);
    }

    private static void CreateIssuerSheet(XLWorkbook workbook, PortfolioSnapshot snapshot)
    {
        var sheet = workbook.Worksheets.Add("По эмитентам");
        var headers = new[] { "Эмитент", "Сектор", "Стоимость", "Валюта", "Доля" };
        WriteHeader(sheet, headers);

        var row = 2;
        foreach (var issuer in snapshot.Issuers)
        {
            sheet.Cell(row, 1).Value = issuer.Issuer;
            sheet.Cell(row, 2).Value = issuer.Sector;
            sheet.Cell(row, 3).Value = issuer.MarketValue;
            sheet.Cell(row, 4).Value = issuer.Currency;
            sheet.Cell(row, 5).Value = issuer.Share;
            row++;
        }

        FormatTable(sheet, row - 1, headers.Length, [3]);
        sheet.Column(5).Style.NumberFormat.Format = "0.00%";
    }

    private static void WriteHeader(IXLWorksheet sheet, IReadOnlyList<string> headers)
    {
        for (var column = 1; column <= headers.Count; column++)
        {
            sheet.Cell(1, column).Value = headers[column - 1];
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("1F4E78");
        sheet.Row(1).Style.Font.FontColor = XLColor.White;
        sheet.SheetView.FreezeRows(1);
    }

    private static void FormatTable(
        IXLWorksheet sheet,
        int lastRow,
        int columns,
        IReadOnlyCollection<int> numericColumns)
    {
        if (lastRow >= 2)
        {
            sheet.Range(1, 1, lastRow, columns).CreateTable();
            foreach (var column in numericColumns)
            {
                sheet.Column(column).Style.NumberFormat.Format = "#,##0.00";
            }
        }

        sheet.Columns().AdjustToContents();
    }

    private static string ToYesNo(bool value)
    {
        return value ? "Да" : "Нет";
    }

    private static string FormatRiskLevel(string? riskLevel)
    {
        return riskLevel switch
        {
            "RISK_LEVEL_LOW" => "Низкий",
            "RISK_LEVEL_MODERATE" => "Средний",
            "RISK_LEVEL_HIGH" => "Высокий",
            "RISK_LEVEL_UNSPECIFIED" or null or "" => "Не указан",
            _ => riskLevel
        };
    }
}
