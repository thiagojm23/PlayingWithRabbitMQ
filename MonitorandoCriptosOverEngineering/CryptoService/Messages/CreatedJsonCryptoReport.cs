using System.Text.Json;

namespace CryptoService.Messages;

internal sealed class CreatedJsonCryptoReport : BaseMessage
{
    public override string ExchangeName => string.Empty;

    public Guid ReportId { get; set; }
    public JsonElement Price { get; set; }
    public JsonElement Trade { get; set; }
    public JsonElement Spreadsheet { get; set; }
}
