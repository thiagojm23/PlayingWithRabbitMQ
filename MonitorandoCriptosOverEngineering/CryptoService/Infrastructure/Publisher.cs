using System.Text.Json;
using CryptoService.Abstractions;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure;

internal sealed class Publisher(IConnection connection) : IPublisher, IAsyncDisposable
{
    private readonly IConnection _connection = connection;
    private IChannel? _channel;

    public async Task PublishMessageAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
    {
        _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties { Persistent = true };

        await _channel.BasicPublishAsync(exchange, routingKey, false, props, body, cancellationToken);
    }

    public async Task PublishMessageAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
    {
        _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var props = new BasicProperties { Persistent = true };

        await _channel.BasicPublishAsync("", queue, false, props, body, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}
