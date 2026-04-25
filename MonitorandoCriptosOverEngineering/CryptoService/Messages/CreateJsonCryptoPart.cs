using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoService.Messages;

internal sealed class CreateJsonCryptoPart : BaseMessage
{
    public override string ExchangeName => string.Empty;

    [JsonPropertyName("Guid")]
    public Guid ReportId { get; set; }

    [JsonPropertyName("TipoDado")]
    public string DataType { get; set; } = string.Empty;

    public JsonElement? Data { get; set; }
    public JsonElement? Dados { get; set; }
    public string? ReplyQueue { get; set; }
    public string? RpcCorrelationId { get; set; }

    public JsonElement? ResolvePayload()
    {
        if (Data is { ValueKind: not JsonValueKind.Undefined })
        {
            return Data.Value;
        }

        if (Dados is { ValueKind: not JsonValueKind.Undefined })
        {
            return Dados.Value;
        }

        return null;
    }
}
