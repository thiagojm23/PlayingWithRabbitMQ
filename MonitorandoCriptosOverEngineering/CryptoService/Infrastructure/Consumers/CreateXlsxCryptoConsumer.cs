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
        var spreadsheetJsonPart = message.CreateSpreadsheetJsonPart(content);

        await _publisher.PublishMessageAsync("create.json.cryptos.queue", spreadsheetJsonPart);

        var publishContexts = CreatePublishContexts(message, content);

        if (publishContexts.Count == 0)
        {
            _logger.LogWarning(
                "Nenhuma notificacao valida foi configurada para o relatorio {ReportType}.",
                message.ReportType);
            return;
        }

        foreach (var publishContext in publishContexts)
        {
            await _publisher.PublishMessageAsync(
                publishContext.Message.ExchangeName,
                publishContext.RoutingKey,
                publishContext.Message);
        }
    }

    private static List<NotificationPublishContext> CreatePublishContexts(CreateXlsxCrypto message, byte[] content)
    {
        var contexts = new List<NotificationPublishContext>();

        if (message.NotifyByEmail && !string.IsNullOrWhiteSpace(message.RecipientEmail))
        {
            contexts.Add(new NotificationPublishContext(
                CreateEmailNotification(message, content),
                BuildRoutingKey("email", message.ReportType)));
        }

        if (message.NotifyBySms && !string.IsNullOrWhiteSpace(message.RecipientPhoneNumber))
        {
            contexts.Add(new NotificationPublishContext(
                CreateSmsNotification(message, content),
                BuildRoutingKey("sms", message.ReportType)));
        }

        return contexts;
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
