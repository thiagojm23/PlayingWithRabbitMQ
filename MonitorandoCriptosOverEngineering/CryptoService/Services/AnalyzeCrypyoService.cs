using System.Globalization;
using CryptoService.Abstractions;
using CryptoService.DTOs.Binance;
using CryptoService.Infrastructure.Spreadsheets;
using CryptoService.Messages;

namespace CryptoService.Services;

internal class AnalyzeCrypyoService(IBinanceClient binanceClient, ILogger<AnalyzeCrypyoService> logger) : IAnalyzeCriptoService
{
    private const int RecentTradesLimit = 200;

    public async Task<CreateXlsxCrypto> AnalyzeCryptosPricesAsync(IEnumerable<string> cryptos)
    {
        var normalizedCryptos = NormalizeCryptos(cryptos);
        var priceRows = new List<PriceSpreadsheetRow>();

        foreach (var crypto in normalizedCryptos)
        {
            IEnumerable<KlineDto> klines;

            try
            {
                klines = await binanceClient.GetKlinesAsync(crypto, "1d", 7);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Erro ao buscar dados de Klines com o client da Binance para {Crypto}", crypto);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro desconhecido ao buscar dados de Klines para {Crypto}", crypto);
                continue;
            }

            foreach (var kline in klines)
            {
                var open = ParseDecimal(kline.Open);
                var high = ParseDecimal(kline.High);
                var low = ParseDecimal(kline.Low);
                var close = ParseDecimal(kline.Close);
                var volume = ParseDecimal(kline.Volume);
                var day = DateTimeOffset.FromUnixTimeMilliseconds(kline.OpenTime).UtcDateTime.Date;
                var variationPct = open == 0 ? 0 : (close - open) / open;

                priceRows.Add(new PriceSpreadsheetRow(
                    crypto,
                    day,
                    open,
                    high,
                    low,
                    close,
                    volume,
                    variationPct));
            }
        }

        return CryptoSpreadsheetMessageFactory.CreatePriceWorkbookMessage(normalizedCryptos, priceRows);
    }

    public async Task<CreateXlsxCrypto> AnalyzeCryptosTradesAsync(IEnumerable<string> cryptos)
    {
        var normalizedCryptos = NormalizeCryptos(cryptos);
        var tradeRows = new List<TradeSpreadsheetRow>();

        foreach (var crypto in normalizedCryptos)
        {
            IEnumerable<RecentTradeDto> recentTrades;

            try
            {
                recentTrades = await binanceClient.GetRecentTradesAsync(crypto, RecentTradesLimit);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Erro ao buscar trades recentes da Binance para {Crypto}", crypto);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro desconhecido ao buscar trades recentes para {Crypto}", crypto);
                continue;
            }

            foreach (var trade in recentTrades.OrderByDescending(item => item.Time))
            {
                tradeRows.Add(new TradeSpreadsheetRow(
                    crypto,
                    trade.Id,
                    DateTimeOffset.FromUnixTimeMilliseconds(trade.Time).UtcDateTime,
                    ParseDecimal(trade.Price),
                    ParseDecimal(trade.Qty),
                    ParseDecimal(trade.QuoteQty),
                    trade.IsBuyerMaker ? "Venda agressora" : "Compra agressora",
                    trade.IsBestMatch ? "Sim" : "Nao"));
            }
        }

        return CryptoSpreadsheetMessageFactory.CreateTradeWorkbookMessage(normalizedCryptos, tradeRows, RecentTradesLimit);
    }

    private static string[] NormalizeCryptos(IEnumerable<string> cryptos) =>
        cryptos
            .Where(crypto => !string.IsNullOrWhiteSpace(crypto))
            .Select(crypto => crypto.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

}
