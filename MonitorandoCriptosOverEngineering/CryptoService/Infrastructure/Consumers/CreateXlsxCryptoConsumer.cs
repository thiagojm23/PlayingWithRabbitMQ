using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class CreateXlsxCryptoConsumer : BasicRabbitMQConsumer<CreateXlsxCrypto, CreateXlsxCryptoConsumer>
{
    private readonly IPublisher _publisher;
    private readonly IXlsxCryptoGeneratorService _xlsxCryptoGeneratorService;
    private readonly ILogger<CreateXlsxCryptoConsumer> _logger;

    public CreateXlsxCryptoConsumer(
        IPublisher publisher,
        IConnection connection,
        IXlsxCryptoGeneratorService xlsxCryptoGeneratorService,
        ILogger<CreateXlsxCryptoConsumer> logger)
        : base(connection, logger, "create.xlsx.cryptos.queue")
    {
        _publisher = publisher;
        _xlsxCryptoGeneratorService = xlsxCryptoGeneratorService;
        _logger = logger;
    }

    protected override async Task OnMessageReceived(CreateXlsxCrypto message)
    {
        var content = await _xlsxCryptoGeneratorService.GenerateAsync(message);
        var publishContext = CreatePublishContext(message, content);

        if (publishContext is null)
        {
            _logger.LogWarning(
                "Nenhuma notificacao valida foi configurada para o relatorio {ReportType}.",
                message.ReportType);
            return;
        }

        await _publisher.PublishMessageAsync(
            publishContext.Message.ExchangeName,
            publishContext.RoutingKey,
            publishContext.Message);
    }

    private NotificationPublishContext? CreatePublishContext(CreateXlsxCrypto message, byte[] content)
    {
        switch (message.NotifyByEmail, message.NotifyBySms)
        {
            case (true, false) when !string.IsNullOrWhiteSpace(message.RecipientEmail):
                return new NotificationPublishContext(
                    CreateEmailNotification(message, content),
                    BuildRoutingKey("email", message.ReportType));
            case (false, true) when !string.IsNullOrWhiteSpace(message.RecipientPhoneNumber):
                return new NotificationPublishContext(
                    CreateSmsNotification(message, content),
                    BuildRoutingKey("sms", message.ReportType));
            default:
                return null;
        }
    }

    private static NotifyEmailCryptoReport CreateEmailNotification(CreateXlsxCrypto message, byte[] content)
    {
        return new NotifyEmailCryptoReport
        {
            SourceMessageId = message.SourceMessageId,
            MessageId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ReportType = message.ReportType,
            WorkbookTitle = message.WorkbookTitle,
            FileName = message.FileName,
            Description = message.Description,
            RecipientEmail = message.RecipientEmail,
            RecipientPhoneNumber = message.RecipientPhoneNumber,
            Content = content
        };
    }

    private static NotifySmsCryptoReport CreateSmsNotification(CreateXlsxCrypto message, byte[] content)
    {
        return new NotifySmsCryptoReport
        {
            SourceMessageId = message.SourceMessageId,
            MessageId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            ReportType = message.ReportType,
            WorkbookTitle = message.WorkbookTitle,
            FileName = message.FileName,
            Description = message.Description,
            RecipientEmail = message.RecipientEmail,
            RecipientPhoneNumber = message.RecipientPhoneNumber,
            Content = content
        };
    }

    private static string BuildRoutingKey(string notificationType, string reportType)
    {
        var normalizedType = notificationType.Trim().ToLowerInvariant();
        var normalizedReportType = reportType.Trim().ToLowerInvariant();

        return $"notify.{normalizedType}.{normalizedReportType}";
    }
}

internal sealed record NotificationPublishContext(
    NotifyCryptoReportMessage Message,
    string RoutingKey);
