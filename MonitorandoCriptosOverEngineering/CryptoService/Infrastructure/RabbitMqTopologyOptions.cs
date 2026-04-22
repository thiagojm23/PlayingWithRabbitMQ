namespace CryptoService.Infrastructure;

internal sealed class RabbitMqTopologyOptions
{
    internal List<QueueDeclaration> Queues { get; } = [];
    internal List<ExchangeDeclaration> Exchanges { get; } = [];
    internal List<BindingDeclaration> Bindings { get; } = [];

    /// <summary>
    /// Declara uma fila standalone — padrão Work Queue.
    /// O RabbitMQ usa a default exchange por baixo, então não precisa declarar exchange.
    /// </summary>
    public RabbitMqTopologyOptions WithQueue(string name, bool durable = true)
    {
        Queues.Add(new QueueDeclaration(name, durable));
        return this;
    }

    /// <summary>
    /// Declara uma exchange (direct, topic, fanout, headers).
    /// Use junto com WithQueue + WithBinding para roteamento avançado.
    /// </summary>
    public RabbitMqTopologyOptions WithExchange(string name, string type, bool durable = true)
    {
        Exchanges.Add(new ExchangeDeclaration(name, type, durable));
        return this;
    }

    /// <summary>
    /// Liga uma fila a uma exchange com uma routing key.
    /// Requer que a exchange e a fila já tenham sido declaradas via WithExchange/WithQueue.
    /// </summary>
    public RabbitMqTopologyOptions WithBinding(string exchange, string queue, string routingKey)
    {
        Bindings.Add(new BindingDeclaration(exchange, queue, routingKey));
        return this;
    }
}

internal record QueueDeclaration(string Name, bool Durable = true, bool Exclusive = false, bool AutoDelete = false);
internal record ExchangeDeclaration(string Name, string Type, bool Durable = true);
internal record BindingDeclaration(string Exchange, string Queue, string RoutingKey);
