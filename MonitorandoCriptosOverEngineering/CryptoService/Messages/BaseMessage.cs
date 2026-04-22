using System.Text.Json.Serialization;

namespace CryptoService.Messages;

internal abstract class BaseMessage
{
    [JsonIgnore]
    public abstract string ExchangeName { get; }

    public Guid MessageId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
