namespace CryptoService.Messages;

internal class NotifyCryptoReportMessage : BaseMessage
{
    public override string ExchangeName => "notify.users.cryptos.topic";

    public Guid SourceMessageId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string WorkbookTitle { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string? Description { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhoneNumber { get; set; }
    public byte[] Content { get; set; } = [];
}
