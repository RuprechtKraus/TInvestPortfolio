# T-Invest Portfolio Exporter

Консольная утилита получает открытые счета и позиции T-Invest, обогащает их сведениями об инструменте и эмитенте, затем сохраняет Excel-отчёт.

## Архитектура

- `src/TInvestPortfolio.Core` — модели приложения, контракты и сценарий экспорта.
- `src/TInvestPortfolio.Infrastructure` — REST-клиент T-Invest и Excel-экспорт через ClosedXML.
- `src/TInvestPortfolio.Cli` — точка входа, Generic Host, DI и конфигурация.
- `tests/TInvestPortfolio.Core.Tests` — unit-тесты NUnit для бизнес-логики.

`Core` не зависит от API T-Invest, ClosedXML, конфигурации или консольного интерфейса.

## Настройка токена

Создайте read-only токен T-Invest и сохраните его в User Secrets:

```powershell
dotnet user-secrets set "TInvest:Token" "ваш-read-only-токен" --project .\src\TInvestPortfolio.Cli
```

Для CI или временного запуска можно использовать переменную окружения:

```powershell
$env:TInvest__Token = "ваш-read-only-токен"
```

Токен не должен храниться в `appsettings.json` и не попадает в репозиторий.

## Запуск

```powershell
dotnet run --project .\src\TInvestPortfolio.Cli -- .\portfolio.xlsx
```

При запуске приложение показывает открытые счета и предлагает выбрать один из них. Затем создаётся отчёт только по выбранному счёту.

Если путь не передан, файл сохраняется в каталог из `Export:OutputDirectory` (`Reports` по умолчанию). В `src/TInvestPortfolio.Cli/appsettings.json` можно указать относительный или абсолютный путь:

```json
{
  "Export": {
    "OutputDirectory": "C:\\Users\\YourName\\Documents\\TInvestReports"
  }
}
```

Аргумент командной строки по-прежнему имеет приоритет над этой настройкой. Настройки в `appsettings.json` загружаются до User Secrets, переменных окружения и аргументов командной строки.

## Проверка

```powershell
dotnet build .\TInvestPortfolio.sln
dotnet test .\TInvestPortfolio.sln
```
