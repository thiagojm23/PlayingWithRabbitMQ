using System.Text.Json;
using CryptoService.Messages;

namespace CryptoService.Services.ComunicationServices;

internal static class SmsNotificationComposer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static string CreateBody(NotifySmsCryptoReport notification)
    {
        return notification.ReportType.Trim().ToLowerInvariant() switch
        {
            "trades" => $"Relatorio de trades pronto: {notification.FileName}. Confira tambem o email, caso habilitado.",
            "prices" => $"Relatorio de precos pronto: {notification.FileName}. Confira tambem o email, caso habilitado.",
            _ => $"Seu relatorio de criptomoedas esta pronto: {notification.FileName}."
        };
    }

    public static string CreateSandboxPayload(NotifySmsCryptoReport notification)
    {
        return JsonSerializer.Serialize(
            new
            {
                notification.MessageId,
                notification.SourceMessageId,
                notification.ReportType,
                notification.FileName,
                notification.RecipientPhoneNumber,
                Body = CreateBody(notification),
                notification.CreatedAt
            },
            JsonOptions);
    }
}
