using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TInvestPortfolio.Infrastructure.Configuration;
using TInvestPortfolio.Infrastructure.TInvest.Models;

namespace TInvestPortfolio.Infrastructure.TInvest;

/// <summary>
/// Выполняет HTTP-запросы к REST API T-Invest.
/// </summary>
public sealed class TInvestClient(HttpClient httpClient, IOptions<TInvestOptions> options) : ITInvestClient
{
    private const string ApiPrefix = "/rest/tinkoff.public.invest.api.contract.v1.";
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    /// <summary>
    /// Получает список открытых счетов T-Invest.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Список контрактов открытых счетов.</returns>
    public async Task<IReadOnlyList<AccountDto>> GetOpenAccountsAsync(CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<AccountsResponse>(
            "UsersService/GetAccounts",
            new { status = "ACCOUNT_STATUS_OPEN" },
            cancellationToken);

        return response.Accounts;
    }

    /// <summary>
    /// Получает позиции портфеля указанного счёта.
    /// </summary>
    /// <param name="accountId">Идентификатор инвестиционного счёта T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Список контрактов позиций портфеля.</returns>
    public async Task<IReadOnlyList<PortfolioPositionDto>> GetPortfolioAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response =
            await PostAsync<PortfolioResponse>("OperationsService/GetPortfolio", new { accountId }, cancellationToken);

        return response.Positions;
    }

    /// <summary>
    /// Получает справочную информацию об инструменте.
    /// </summary>
    /// <param name="instrumentUid">Уникальный идентификатор инструмента T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Контракт справочных данных инструмента.</returns>
    public async Task<InstrumentDto> GetInstrumentAsync(
        string instrumentUid,
        CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<InstrumentResponse>(
            "InstrumentsService/GetInstrumentBy",
            new
            {
                idType = "INSTRUMENT_ID_TYPE_UID", id = instrumentUid
            },
            cancellationToken);

        return response.Instrument;
    }

    /// <summary>
    /// Получает подробные данные актива.
    /// </summary>
    /// <param name="assetUid">Уникальный идентификатор актива T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Контракт актива или <see langword="null"/>, если API не вернул данные.</returns>
    public async Task<AssetDto?> GetAssetAsync(string assetUid, CancellationToken cancellationToken = default)
    {
        var response =
            await PostAsync<AssetResponse>("InstrumentsService/GetAssetBy", new { id = assetUid }, cancellationToken);

        return response.Asset;
    }

    /// <summary>
    /// Получает параметры облигации.
    /// </summary>
    /// <param name="instrumentUid">Уникальный идентификатор облигации T-Invest.</param>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса.</param>
    /// <returns>Контракт параметров облигации или <see langword="null"/>, если API не вернул данные.</returns>
    public async Task<BondDto?> GetBondAsync(string instrumentUid, CancellationToken cancellationToken = default)
    {
        var response = await PostAsync<BondResponse>(
            "InstrumentsService/BondBy",
            new { idType = "INSTRUMENT_ID_TYPE_UID", id = instrumentUid },
            cancellationToken);

        return response.Instrument;
    }

    private async Task<TResponse> PostAsync<TResponse>(
        string method,
        object request,
        CancellationToken cancellationToken)
    {
        ConfigureAuthorizationHeader();
        using var response =
            await httpClient.PostAsJsonAsync($"{ApiPrefix}{method}", request, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken) ??
               throw new InvalidOperationException($"T-Invest returned an empty response for {method}.");
    }

    private void ConfigureAuthorizationHeader()
    {
        var token = options.Value.Token;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "T-Invest token is not configured. Set TInvest:Token via User Secrets or TInvest__Token.");
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
