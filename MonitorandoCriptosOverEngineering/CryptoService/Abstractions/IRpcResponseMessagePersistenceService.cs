using CryptoService.Contracts;

namespace CryptoService.Abstractions;

internal interface IRpcResponseMessagePersistenceService
{
    Task PersistAsync<TRpcReturn>(
        RpcPublishResponse<TRpcReturn> response,
        CancellationToken cancellationToken = default);
}
