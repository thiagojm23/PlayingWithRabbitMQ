using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class NotifyEmailCryptosConsumer(
    IEmailNotificationService emailNotificationService,
    IConnection connection,
    ILogger<NotifyEmailCryptosConsumer> logger)
    : BasicRabbitMQConsumer<NotifyEmailCryptoReport, NotifyEmailCryptosConsumer>(connection, logger, "notify.email.cryptos.queue")
{
    protected override Task OnMessageReceived(NotifyEmailCryptoReport message)
    {
        return emailNotificationService.SendAsync(message);
    }
}
