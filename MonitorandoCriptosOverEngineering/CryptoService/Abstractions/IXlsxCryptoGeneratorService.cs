using CryptoService.Messages;

namespace CryptoService.Abstractions;

internal interface IXlsxCryptoGeneratorService
{
    Task<byte[]> GenerateAsync(CreateXlsxCrypto message, CancellationToken cancellationToken = default);
}
