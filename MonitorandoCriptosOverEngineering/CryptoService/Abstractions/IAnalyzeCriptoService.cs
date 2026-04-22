using CryptoService.Messages;

namespace CryptoService.Abstractions;

internal interface IAnalyzeCriptoService
{
    Task<CreateXlsxCrypto> AnalyzeCryptosPricesAsync(IEnumerable<string> cryptos);
    Task<CreateXlsxCrypto> AnalyzeCryptosTradesAsync(IEnumerable<string> cryptos);
}
