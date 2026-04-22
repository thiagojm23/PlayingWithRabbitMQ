using CryptoService.Abstractions;
using CryptoService.Messages;
using CryptoService.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace CryptoService.Services.ComunicationServices;

internal sealed class EmailNotificationService(
    IOptions<GmailSmtpSettings> settings,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    private readonly GmailSmtpSettings _settings = settings.Value;

    public async Task SendAsync(NotifyEmailCryptoReport message, CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var mimeMessage = EmailNotificationComposer.CreateMessage(
            message,
            _settings.FromEmail,
            _settings.FromName);

        using var smtpClient = new SmtpClient();
        var socketOptions = _settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

        await smtpClient.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);
        await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        await smtpClient.SendAsync(mimeMessage, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);

        logger.LogInformation(
            "Email enviado para {RecipientEmail} referente ao relatorio {ReportType}.",
            message.RecipientEmail,
            message.ReportType);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Password) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException(
                "As configuracoes de email nao foram informadas. Configure Notifications:Gmail via user-secrets.");
        }
    }
}
