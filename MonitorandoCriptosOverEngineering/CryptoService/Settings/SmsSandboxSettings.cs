namespace CryptoService.Settings;

internal sealed class SmsSandboxSettings
{
    public const string SectionName = "Notifications:SmsSandbox";

    public string OutboxDirectory { get; set; } = "sms-outbox";
}
