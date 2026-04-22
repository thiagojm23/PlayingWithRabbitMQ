namespace CryptoService.Messages;

internal class ReportsCryptosTrades : BaseMessage
{
    public override string ExchangeName => "request.reports.cryptos.exchange";

    public IEnumerable<string> Cryptos { get; set; } = [];
    public bool NotifyByEmail { get; set; }
    public bool NotifyBySms { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhoneNumber { get; set; }
}
