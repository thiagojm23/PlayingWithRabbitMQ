using CryptoService.Messages;

namespace CryptoService.Abstractions;

internal interface ICreateJsonCryptoAggregationService
{
    Task<CreatedJsonCryptoReport?> StoreAndTryCompleteAsync(
        CreateJsonCryptoPart message,
        CancellationToken cancellationToken = default);
}
