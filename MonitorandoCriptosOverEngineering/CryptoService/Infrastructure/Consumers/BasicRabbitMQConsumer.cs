using System.Text.Json;
using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CryptoService.Infrastructure.Consumers;

internal abstract class BasicRabbitMQConsumer<T, TConsumer>(IConnection connection, ILogger<TConsumer> logger, string queueName)
    : IRabbitMqConsumer
    where T : BaseMessage
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected readonly IChannel _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

    protected virtual ushort PrefetchCount => 1;
    protected virtual bool Durable => true;
    protected virtual bool AutoAck => false;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _channel.BasicQosAsync(0, PrefetchCount, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(body, JsonOptions);

                if (message == null)
                {
                    logger.LogError("Payload nulo recebido.");
                    await _channel.BasicRejectAsync(ea.DeliveryTag, requeue: false, cancellationToken: cancellationToken);
                    return;
                }

                await OnMessageReceived(message);

                if (!AutoAck) await _channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
            }
            catch (JsonException jsonEx)
            {
                logger.LogError(jsonEx, "Erro de desserialização: {mensagem}", typeof(T));
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar a mensagem {mensagem}.", typeof(T));
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, cancellationToken: cancellationToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: AutoAck, consumer: consumer, cancellationToken);
    }

    protected abstract Task OnMessageReceived(T message);
}
