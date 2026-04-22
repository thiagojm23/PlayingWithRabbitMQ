namespace CryptoService.Infrastructure.Consumers;

internal interface IRabbitMqConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}
