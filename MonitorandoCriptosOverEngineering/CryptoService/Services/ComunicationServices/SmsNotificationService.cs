using System.Text;
using CryptoService.Abstractions;
using CryptoService.Messages;
using CryptoService.Settings;
using Microsoft.Extensions.Options;

namespace CryptoService.Services.ComunicationServices;

internal sealed class SmsNotificationService(
    IOptions<SmsSandboxSettings> settings,
    ILogger<SmsNotificationService> logger) : ISmsNotificationService
{
    private readonly SmsSandboxSettings _settings = settings.Value;

    public async Task SendAsync(NotifySmsCryptoReport message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message.RecipientPhoneNumber))
        {
            throw new InvalidOperationException("RecipientPhoneNumber deve ser informado para notificacao SMS.");
        }

        var outboxDirectory = ResolveOutboxDirectory(_settings.OutboxDirectory);
        Directory.CreateDirectory(outboxDirectory);

        var fileName = $"sms-{DateTime.UtcNow:yyyyMMddHHmmss}-{message.MessageId:N}.json";
        var payloadPath = Path.Combine(outboxDirectory, fileName);
        var payload = SmsNotificationComposer.CreateSandboxPayload(message);

        await File.WriteAllTextAsync(payloadPath, payload, Encoding.UTF8, cancellationToken);

        logger.LogInformation(
            "SMS de teste registrado em {PayloadPath} para {RecipientPhoneNumber}.",
            payloadPath,
            message.RecipientPhoneNumber);
    }

    private static string ResolveOutboxDirectory(string configuredDirectory) =>
        Path.IsPathRooted(configuredDirectory)
            ? configuredDirectory
            : Path.Combine(AppContext.BaseDirectory, configuredDirectory);
}
