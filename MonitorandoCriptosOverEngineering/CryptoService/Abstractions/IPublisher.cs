namespace CryptoService.Abstractions;

internal interface IPublisher
{
    Task PublishMessageAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default);
}
