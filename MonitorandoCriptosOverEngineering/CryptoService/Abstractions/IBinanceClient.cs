using CryptoService.DTOs.Binance;

namespace CryptoService.Abstractions;

internal interface IBinanceClient
{
    Task<SymbolPriceTickerDto?> GetCurrentPriceAsync(string symbol);
    Task<IEnumerable<RecentTradeDto>> GetRecentTradesAsync(string symbol, int limit = 500);
    Task<IEnumerable<KlineDto>> GetKlinesAsync(string symbol, string interval, int limit);
}
