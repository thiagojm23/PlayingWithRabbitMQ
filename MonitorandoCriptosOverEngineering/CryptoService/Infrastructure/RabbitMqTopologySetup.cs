using RabbitMQ.Client;

namespace CryptoService.Infrastructure;

/// <summary>
/// Roda uma única vez no startup da aplicação para declarar exchanges, filas e bindings.
/// Usa IHostedService (não BackgroundService) porque não é uma tarefa contínua — apenas setup inicial.
/// </summary>
internal sealed class RabbitMqTopologySetup(IConnection connection, RabbitMqTopologyOptions options) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Canal descartável — só usado para declaração, não para publicação contínua
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        foreach (var exchange in options.Exchanges)
            await channel.ExchangeDeclareAsync(exchange.Name, exchange.Type, exchange.Durable, cancellationToken: cancellationToken);

        foreach (var queue in options.Queues)
            await channel.QueueDeclareAsync(queue.Name, queue.Durable, queue.Exclusive, queue.AutoDelete, cancellationToken: cancellationToken);

        foreach (var binding in options.Bindings)
            await channel.QueueBindAsync(binding.Queue, binding.Exchange, binding.RoutingKey, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
