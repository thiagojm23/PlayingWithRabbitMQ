using CryptoService.Messages;

namespace CryptoService.Abstractions;

internal interface IEmailNotificationService
{
    Task SendAsync(NotifyEmailCryptoReport message, CancellationToken cancellationToken = default);
}
