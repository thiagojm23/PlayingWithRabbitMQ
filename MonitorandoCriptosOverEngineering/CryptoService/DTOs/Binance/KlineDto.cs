namespace CryptoService.DTOs.Binance;

internal class KlineDto
{
    public long OpenTime { get; set; }
    public string? Open { get; set; }
    public string? High { get; set; }
    public string? Low { get; set; }
    public string? Close { get; set; }
    public string? Volume { get; set; }
    public long CloseTime { get; set; }
}
