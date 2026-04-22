using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class NotifySaveLogsCryptosConsumer(
    INotificationLogPersistenceService notificationLogPersistenceService,
    IConnection connection,
    ILogger<NotifySaveLogsCryptosConsumer> logger)
    : BasicRabbitMQConsumer<NotifyCryptoReportMessage, NotifySaveLogsCryptosConsumer>(connection, logger, "notify.save.logs.cryptos.queue")
{
    protected override Task OnMessageReceived(NotifyCryptoReportMessage message)
    {
        return notificationLogPersistenceService.PersistAsync(message);
    }
}
