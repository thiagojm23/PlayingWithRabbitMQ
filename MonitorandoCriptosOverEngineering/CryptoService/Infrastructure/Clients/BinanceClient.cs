using System.Text.Json;
using CryptoService.Abstractions;
using CryptoService.DTOs.Binance;

namespace CryptoService.Infrastructure.Clients;

internal class BinanceClient(HttpClient httpClient) : IBinanceClient
{
    private readonly HttpClient _httpClient = httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<SymbolPriceTickerDto?> GetCurrentPriceAsync(string symbol)
    {
        var response = await _httpClient.GetAsync($"api/v3/ticker/price?symbol={symbol}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SymbolPriceTickerDto>(content, _jsonOptions);
    }

    public async Task<IEnumerable<RecentTradeDto>> GetRecentTradesAsync(string symbol, int limit = 500)
    {
        var response = await _httpClient.GetAsync($"api/v3/trades?symbol={symbol}&limit={limit}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<RecentTradeDto>>(content, _jsonOptions) ?? [];
    }

    public async Task<IEnumerable<KlineDto>> GetKlinesAsync(string symbol, string interval, int limit)
    {
        var response = await _httpClient.GetAsync($"api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        // A API da Binance retorna Klines como um array de arrays: [ [ openTime, open, high, low, close, volume, closeTime, ... ], ... ]
        using var document = JsonDocument.Parse(content);
        var klines = new List<KlineDto>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            klines.Add(new KlineDto
            {
                OpenTime = element[0].GetInt64(),
                Open = element[1].GetString() ?? string.Empty,
                High = element[2].GetString() ?? string.Empty,
                Low = element[3].GetString() ?? string.Empty,
                Close = element[4].GetString() ?? string.Empty,
                Volume = element[5].GetString() ?? string.Empty,
                CloseTime = element[6].GetInt64()
            });
        }

        return klines;
    }
}
