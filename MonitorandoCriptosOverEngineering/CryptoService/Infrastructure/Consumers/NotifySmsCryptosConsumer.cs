using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class NotifySmsCryptosConsumer(
    ISmsNotificationService smsNotificationService,
    IConnection connection,
    ILogger<NotifySmsCryptosConsumer> logger)
    : BasicRabbitMQConsumer<NotifySmsCryptoReport, NotifySmsCryptosConsumer>(connection, logger, "notify.sms.cryptos.queue")
{
    protected override Task OnMessageReceived(NotifySmsCryptoReport message)
    {
        return smsNotificationService.SendAsync(message);
    }
}
