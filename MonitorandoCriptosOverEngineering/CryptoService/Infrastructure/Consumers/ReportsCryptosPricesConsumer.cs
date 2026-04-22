using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal class ReportsCryptosPricesConsumer(IPublisher publisher,
    IConnection connection,
    IAnalyzeCriptoService analyzeCriptoService,
    ILogger<ReportsCryptosPricesConsumer> logger)
    : BasicRabbitMQConsumer<ReportsCryptosPrices, ReportsCryptosPricesConsumer>(connection,
        logger, "reports.cryptos.prices.queue")
{
    protected override async Task OnMessageReceived(ReportsCryptosPrices message)
    {
        var createXlsxMessage = await analyzeCriptoService.AnalyzeCryptosPricesAsync(message.Cryptos);
        createXlsxMessage.SourceMessageId = message.MessageId;
        createXlsxMessage.NotifyByEmail = message.NotifyByEmail;
        createXlsxMessage.NotifyBySms = message.NotifyBySms;
        createXlsxMessage.RecipientEmail = message.RecipientEmail;
        createXlsxMessage.RecipientPhoneNumber = message.RecipientPhoneNumber;

        await publisher.PublishMessageAsync(
            createXlsxMessage.ExchangeName,
            "xlsx.cryptos",
            createXlsxMessage);
    }
}
