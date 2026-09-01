namespace TInvestPortfolio.Infrastructure.TInvest;

/// <summary>
/// Преобразует технические названия секторов T-Invest в русскоязычные подписи.
/// </summary>
internal static class SectorLocalizer
{
    /// <summary>
    /// Возвращает русскоязычное название сектора или исходное неизвестное значение.
    /// </summary>
    /// <param name="sector">Техническое название сектора из T-Invest API.</param>
    /// <returns>Русскоязычное название сектора либо «Не указан» для пустого значения.</returns>
    public static string ToRussian(string? sector)
    {
        if (string.IsNullOrWhiteSpace(sector))
        {
            return "Не указан";
        }

        var normalizedSector = sector.Trim().Replace('_', ' ').ToLowerInvariant();

        return normalizedSector switch
        {
            "financial" or "finance" or "financials" => "Финансы",
            "energy" => "Энергетика",
            "materials" or "basic materials" => "Сырьё и материалы",
            "industrials" or "industrial" => "Промышленность",
            "utilities" => "Коммунальные услуги",
            "technology" or "information technology" or "it" => "Информационные технологии",
            "telecom" or "telecommunications" or "communication services" => "Телекоммуникации",
            "consumer" or "consumer services" => "Потребительский сектор",
            "consumer cyclical" or "consumer discretionary" => "Товары длительного спроса",
            "consumer defensive" or "consumer staples" => "Товары первой необходимости",
            "healthcare" or "health care" => "Здравоохранение",
            "real estate" => "Недвижимость",
            "transportation" => "Транспорт",
            "retail" => "Розничная торговля",
            "other" => "Прочее",
            _ => sector
        };
    }
}
