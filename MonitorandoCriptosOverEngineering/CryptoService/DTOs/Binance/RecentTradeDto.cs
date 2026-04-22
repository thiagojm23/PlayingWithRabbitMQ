namespace CryptoService.DTOs.Binance;

internal class RecentTradeDto
{
    public long Id { get; set; }
    public string? Price { get; set; }
    public string? Qty { get; set; }
    public string? QuoteQty { get; set; }
    public long Time { get; set; }
    public bool IsBuyerMaker { get; set; }
    public bool IsBestMatch { get; set; }
}
