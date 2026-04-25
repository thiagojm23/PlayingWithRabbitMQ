using CryptoService.Abstractions;
using CryptoService.Messages;
using Microsoft.Data.Sqlite;

namespace CryptoService.Services.ComunicationServices;

internal sealed class SqliteNotificationLogPersistenceService(
    ISqliteConnectionFactory sqliteConnectionFactory,
    ILogger<SqliteNotificationLogPersistenceService> logger) : INotificationLogPersistenceService
{
    public async Task PersistAsync(NotifyCryptoReportMessage message, CancellationToken cancellationToken = default)
    {
        await using var connection = await sqliteConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO notification_dispatch_events
            (
                message_id,
                source_message_id,
                report_type,
                workbook_title,
                file_name,
                content_type,
                content_length,
                recipient_email,
                recipient_phone_number,
                notify_by_email,
                notify_by_sms,
                created_at_utc,
                persisted_at_utc
            )
            VALUES
            (
                $messageId,
                $sourceMessageId,
                $reportType,
                $workbookTitle,
                $fileName,
                $contentType,
                $contentLength,
                $recipientEmail,
                $recipientPhoneNumber,
                $notifyByEmail,
                $notifyBySms,
                $createdAtUtc,
                $persistedAtUtc
            );
            """;

        command.Parameters.AddWithValue("$messageId", message.MessageId.ToString("D"));
        command.Parameters.AddWithValue("$sourceMessageId", message.SourceMessageId.ToString("D"));
        command.Parameters.AddWithValue("$reportType", message.ReportType);
        command.Parameters.AddWithValue("$workbookTitle", message.WorkbookTitle);
        command.Parameters.AddWithValue("$fileName", message.FileName);
        command.Parameters.AddWithValue("$contentType", message.ContentType);
        command.Parameters.AddWithValue("$contentLength", message.Content.Length);
        command.Parameters.AddWithValue("$recipientEmail", (object?)message.RecipientEmail ?? DBNull.Value);
        command.Parameters.AddWithValue("$recipientPhoneNumber", (object?)message.RecipientPhoneNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("$notifyByEmail", message is NotifyEmailCryptoReport ? 1 : 0);
        command.Parameters.AddWithValue("$notifyBySms", message is NotifySmsCryptoReport ? 1 : 0);
        command.Parameters.AddWithValue("$createdAtUtc", message.CreatedAt);
        command.Parameters.AddWithValue("$persistedAtUtc", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);

        logger.LogInformation(
            "Evento de notificacao persistido no SQLite para o relatorio {ReportType}.",
            message.ReportType);
    }

    private static async Task EnsureSchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS notification_dispatch_events
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                message_id TEXT NOT NULL,
                source_message_id TEXT NOT NULL,
                report_type TEXT NOT NULL,
                workbook_title TEXT NOT NULL,
                file_name TEXT NOT NULL,
                content_type TEXT NOT NULL,
                content_length INTEGER NOT NULL,
                recipient_email TEXT NULL,
                recipient_phone_number TEXT NULL,
                notify_by_email INTEGER NOT NULL,
                notify_by_sms INTEGER NOT NULL,
                created_at_utc TEXT NOT NULL,
                persisted_at_utc TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
