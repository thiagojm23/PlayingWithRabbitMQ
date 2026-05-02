using System.Text.Json;
using CryptoService.Contracts;

namespace CryptoService.Messages;

internal sealed class CreatedJsonCryptoReport : BaseMessage
{
    public override string ExchangeName => string.Empty;

    public Guid ReportId { get; set; }
    public int PriceRowCount { get; set; }
    public int TradeRowCount { get; set; }
    public ReportPreviewSummary PricePreview { get; set; } = new();
    public ReportPreviewSummary TradePreview { get; set; } = new();
    public JsonElement Price { get; set; }
    public JsonElement Trade { get; set; }
    public JsonElement Spreadsheet { get; set; }
}
