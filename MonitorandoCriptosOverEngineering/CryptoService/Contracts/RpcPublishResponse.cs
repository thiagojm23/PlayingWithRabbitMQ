namespace CryptoService.Contracts;

internal sealed class RpcPublishResponse<TRpcReturn>
{
    public required string CorrelationId { get; init; }
    public required string ReplyQueueName { get; init; }
    public required string RpcRequestQueue { get; init; }
    public required TRpcReturn RpcReturn { get; init; }
}
