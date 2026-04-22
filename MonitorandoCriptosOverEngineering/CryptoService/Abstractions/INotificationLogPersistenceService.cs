using CryptoService.Messages;

namespace CryptoService.Abstractions;

internal interface INotificationLogPersistenceService
{
    Task PersistAsync(NotifyCryptoReportMessage message, CancellationToken cancellationToken = default);
}
