using CryptoService.Messages;

namespace CryptoService.Abstractions;

internal interface ISmsNotificationService
{
    Task SendAsync(NotifySmsCryptoReport message, CancellationToken cancellationToken = default);
}
