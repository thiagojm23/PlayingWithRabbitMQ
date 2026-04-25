using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Messages;

internal sealed class CreatedJsonCryptoReport : BaseMessage
{
    public override string ExchangeName => "created.json.cryptos.direct";

    public Guid ReportId { get; set; }
    public string? RpcCorrelationId { get; set; }
    public JsonElement Price { get; set; }
    public JsonElement Trade { get; set; }
    public JsonElement Spreadsheet { get; set; }

    [JsonIgnore]
    public string? ReplyQueue { get; set; }
}
