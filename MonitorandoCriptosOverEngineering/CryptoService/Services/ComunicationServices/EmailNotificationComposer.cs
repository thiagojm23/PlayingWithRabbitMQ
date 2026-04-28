using CryptoService.Messages;
using MimeKit;

namespace CryptoService.Services.ComunicationServices;

internal static class EmailNotificationComposer
{
    public static MimeMessage CreateMessage(NotifyEmailCryptoReport notification, string fromEmail, string fromName)
    {
        if (string.IsNullOrWhiteSpace(notification.RecipientEmail))
        {
            throw new InvalidOperationException("RecipientEmail deve ser informado para envio de email.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(notification.RecipientEmail));
        message.Subject = CreateSubject(notification);

        var builder = new BodyBuilder
        {
            TextBody = CreateBody(notification)
        };

        builder.Attachments.Add(
            notification.FileName,
            new MemoryStream(notification.Content, writable: false),
            ContentType.Parse(notification.ContentType));

        message.Body = builder.ToMessageBody();
        return message;
    }

    private static string CreateSubject(NotifyEmailCryptoReport notification)
    {
        return notification.ReportType.Trim().ToLowerInvariant() switch
        {
            "trades" => "Sua planilha de trades esta pronta",
            "prices" => "Sua planilha de precos esta pronta",
            _ => "Sua planilha de criptomoedas esta pronta"
        };
    }

    private static string CreateBody(NotifyEmailCryptoReport notification)
    {
        return $$"""
        Ola,

        A planilha "{{notification.WorkbookTitle}}" foi gerada com sucesso.

        Tipo do relatorio: {{notification.ReportType}}
        Arquivo anexado: {{notification.FileName}}

        {{notification.Description ?? "A planilha segue anexada neste email."}}

        Atenciosamente,
        CryptoService
        """;
    }
}
