using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal class ReportsCryptosTradesConsumer(
    IPublisher publisher,
    IConnection connection,
    IAnalyzeCriptoService analyzeCriptoService,
    ILogger<ReportsCryptosTradesConsumer> logger)
    : BasicRabbitMQConsumer<ReportsCryptosTrades, ReportsCryptosTradesConsumer>(connection,
        logger, "reports.cryptos.trades.queue")
{
    protected override async Task OnMessageReceived(ReportsCryptosTrades message)
    {
        var createXlsxMessage = await analyzeCriptoService.AnalyzeCryptosTradesAsync(message.Cryptos);
        createXlsxMessage.SourceMessageId = message.MessageId;
        createXlsxMessage.NotifyByEmail = message.NotifyByEmail;
        createXlsxMessage.NotifyBySms = message.NotifyBySms;
        createXlsxMessage.RecipientEmail = message.RecipientEmail;
        createXlsxMessage.RecipientPhoneNumber = message.RecipientPhoneNumber;

        await publisher.PublishMessageAsync(
            "create.json.cryptos.queue",
            createXlsxMessage.CreateJsonPart("Trade"));

        await publisher.PublishMessageAsync(
            createXlsxMessage.ExchangeName,
            "xlsx.cryptos",
            createXlsxMessage);
    }
}
