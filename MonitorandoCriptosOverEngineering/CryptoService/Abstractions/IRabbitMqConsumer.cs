namespace CryptoService.Abstractions;

internal interface IRabbitMqConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}
