using System.Text.Json;
using CryptoService.Abstractions;
using CryptoService.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CryptoService.Infrastructure;

internal sealed class Publisher(
    IConnection connection,
    ILogger<Publisher> logger) : IPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan DefaultRpcTimeout = TimeSpan.FromSeconds(2);

    private readonly IConnection _connection = connection;
    private readonly ILogger<Publisher> _logger = logger;
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private IChannel? _channel;

    public async Task PublishMessageAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
    {
        await _publishLock.WaitAsync(cancellationToken);

        try
        {
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true
            };

            await _channel.BasicPublishAsync(exchange, routingKey, false, props, body, cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async Task PublishMessageAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
    {
        await _publishLock.WaitAsync(cancellationToken);

        try
        {
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true
            };

            await _channel.BasicPublishAsync("", queue, false, props, body, cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async Task<RpcPublishResponse<TResponse>> PublishRpcMessageAsync<TRequest, TResponse>(
        string queue,
        TRequest message,
        string replyQueueBaseName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var rpcTimeout = timeout ?? DefaultRpcTimeout;
        var replyQueueName = CreateReplyQueueName(replyQueueBaseName);
        var correlationId = Guid.NewGuid().ToString("N");
        var responseCompletion = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var rpcChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await rpcChannel.QueueDeclareAsync(
            queue: replyQueueName,
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(rpcChannel);
        consumer.ReceivedAsync += (_, ea) =>
        {
            if (ea.BasicProperties.CorrelationId != correlationId)
            {
                return Task.CompletedTask;
            }

            try
            {
                var response = JsonSerializer.Deserialize<TResponse>(ea.Body.Span, JsonOptions);

                if (response is null)
                {
                    responseCompletion.TrySetException(new JsonException("Resposta RPC nula."));
                    return Task.CompletedTask;
                }

                responseCompletion.TrySetResult(response);
            }
            catch (Exception ex)
            {
                responseCompletion.TrySetException(ex);
            }

            return Task.CompletedTask;
        };

        var consumerTag = await rpcChannel.BasicConsumeAsync(
            queue: replyQueueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken);

        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                CorrelationId = correlationId,
                ReplyTo = replyQueueName
            };

            await rpcChannel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(rpcTimeout);

            try
            {
                var rpcReturn = await responseCompletion.Task.WaitAsync(timeoutCts.Token);
                return new RpcPublishResponse<TResponse>
                {
                    CorrelationId = correlationId,
                    ReplyQueueName = replyQueueName,
                    RpcRequestQueue = queue,
                    RpcReturn = rpcReturn
                };
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"RPC para a fila '{queue}' excedeu o limite de {rpcTimeout.TotalSeconds:N0}s. ReplyTo: '{replyQueueName}'. CorrelationId: '{correlationId}'.");
            }
        }
        finally
        {
            await TryCancelConsumerAsync(rpcChannel, consumerTag, CancellationToken.None);
            await TryDeleteQueueAsync(rpcChannel, replyQueueName, CancellationToken.None);
        }
    }

    public async Task PublishRpcReplyAsync<T>(
        string replyQueue,
        string correlationId,
        T message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(replyQueue))
        {
            throw new ArgumentException("A fila ReplyTo precisa ser informada.", nameof(replyQueue));
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("O CorrelationId precisa ser informado.", nameof(correlationId));
        }

        await _publishLock.WaitAsync(cancellationToken);

        try
        {
            _channel ??= await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            var props = new BasicProperties
            {
                ContentType = "application/json",
                CorrelationId = correlationId
            };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: replyQueue,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    private static string CreateReplyQueueName(string baseName)
    {
        var safeBaseName = string.Concat(
            baseName.Trim().Select(character =>
                char.IsLetterOrDigit(character) || character is '.' or '-' or '_'
                    ? character
                    : '-'));

        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "rpc";
        }

        if (safeBaseName.Length > 80)
        {
            safeBaseName = safeBaseName[..80];
        }

        return $"rpc.reply.{safeBaseName}.{Environment.ProcessId}.{Guid.NewGuid():N}";
    }

    private async Task TryCancelConsumerAsync(IChannel channel, string consumerTag, CancellationToken cancellationToken)
    {
        try
        {
            await channel.BasicCancelAsync(consumerTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nao foi possivel cancelar o consumer RPC {ConsumerTag}.", consumerTag);
        }
    }

    private async Task TryDeleteQueueAsync(IChannel channel, string queueName, CancellationToken cancellationToken)
    {
        try
        {
            await channel.QueueDeleteAsync(queueName, ifUnused: false, ifEmpty: false, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Nao foi possivel excluir a fila RPC ReplyTo {QueueName}.", queueName);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        _publishLock.Dispose();
    }
}
