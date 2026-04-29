using CryptoService.Contracts;

namespace CryptoService.Abstractions;

internal interface IPublisher
{
    Task PublishMessageAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default);
    Task PublishMessageAsync<T>(string queue, T message, CancellationToken cancellationToken = default);
    Task<RpcPublishResponse<TResponse>> PublishRpcMessageAsync<TRequest, TResponse>(
        string queue,
        TRequest message,
        string replyQueueBaseName,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);
    Task PublishRpcReplyAsync<T>(
        string replyQueue,
        string correlationId,
        T message,
        CancellationToken cancellationToken = default);
}
