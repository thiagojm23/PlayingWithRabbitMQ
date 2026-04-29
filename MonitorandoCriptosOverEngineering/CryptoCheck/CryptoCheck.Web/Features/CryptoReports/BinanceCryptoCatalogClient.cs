using System.Text.Json.Serialization;

namespace CryptoCheck.Web.Features.CryptoReports;

public sealed class BinanceCryptoCatalogClient(HttpClient httpClient)
{
    private static readonly IReadOnlyDictionary<string, string> KnownAssetNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["BTC"] = "Bitcoin",
        ["ETH"] = "Ethereum",
        ["BNB"] = "BNB",
        ["SOL"] = "Solana",
        ["XRP"] = "XRP",
        ["ADA"] = "Cardano",
        ["DOGE"] = "Dogecoin",
        ["TRX"] = "TRON",
        ["TON"] = "Toncoin",
        ["AVAX"] = "Avalanche",
        ["LINK"] = "Chainlink",
        ["DOT"] = "Polkadot",
        ["MATIC"] = "Polygon",
        ["LTC"] = "Litecoin",
        ["BCH"] = "Bitcoin Cash"
    };

    public async Task<IReadOnlyList<CryptoOption>> GetTradableUsdtCryptosAsync(CancellationToken cancellationToken = default)
    {
        var exchangeInfo = await httpClient.GetFromJsonAsync<ExchangeInfoResponse>(
            "api/v3/exchangeInfo?symbolStatus=TRADING",
            cancellationToken);

        return exchangeInfo?.Symbols
            .Where(symbol => symbol.Status == "TRADING"
                && symbol.QuoteAsset.Equals("USDT", StringComparison.OrdinalIgnoreCase)
                && symbol.IsSpotTradingAllowed)
            .Select(symbol => new CryptoOption(
                symbol.Symbol,
                symbol.BaseAsset,
                KnownAssetNames.GetValueOrDefault(symbol.BaseAsset, symbol.BaseAsset)))
            .DistinctBy(option => option.Symbol)
            .OrderBy(option => option.Code)
            .ToArray() ?? [];
    }

    private sealed record ExchangeInfoResponse(IReadOnlyList<ExchangeSymbol> Symbols);

    private sealed record ExchangeSymbol(
        string Symbol,
        string Status,
        string BaseAsset,
        string QuoteAsset,
        [property: JsonPropertyName("isSpotTradingAllowed")] bool IsSpotTradingAllowed);
}

public sealed record CryptoOption(string Symbol, string Code, string Name);
