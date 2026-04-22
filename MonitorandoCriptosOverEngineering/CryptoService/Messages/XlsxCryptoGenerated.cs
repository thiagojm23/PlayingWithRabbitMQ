namespace CryptoService.Messages;

internal sealed class XlsxCryptoGenerated : BaseMessage
{
    public override string ExchangeName => "created.docs.cryptos.direct";

    public Guid SourceCreateMessageId { get; set; }
    public string WorkbookTitle { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public byte[] Content { get; set; } = [];
}
