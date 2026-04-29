using CryptoService.Abstractions;
using CryptoService.Contracts;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace CryptoService.Services.ComunicationServices;

internal sealed class SqliteRpcResponseMessagePersistenceService(
    ISqliteConnectionFactory sqliteConnectionFactory,
    ILogger<SqliteRpcResponseMessagePersistenceService> logger) : IRpcResponseMessagePersistenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PersistAsync<TRpcReturn>(
        RpcPublishResponse<TRpcReturn> response,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await sqliteConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO rpc_response_messages
            (
                correlation_id,
                reply_queue_name,
                rpc_return_message,
                rpc_request_queue,
                persisted_at_utc
            )
            VALUES
            (
                $correlationId,
                $replyQueueName,
                $rpcReturnMessage,
                $rpcRequestQueue,
                $persistedAtUtc
            );
            """;

        command.Parameters.AddWithValue("$correlationId", response.CorrelationId);
        command.Parameters.AddWithValue("$replyQueueName", response.ReplyQueueName);
        command.Parameters.AddWithValue("$rpcReturnMessage", JsonSerializer.Serialize(response.RpcReturn, JsonOptions));
        command.Parameters.AddWithValue("$rpcRequestQueue", response.RpcRequestQueue);
        command.Parameters.AddWithValue("$persistedAtUtc", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);

        logger.LogInformation(
            "Resposta RPC persistida no SQLite. CorrelationId: {CorrelationId}. Queue: {RpcRequestQueue}.",
            response.CorrelationId,
            response.RpcRequestQueue);
    }

    private static async Task EnsureSchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS rpc_response_messages
            (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                correlation_id TEXT NOT NULL,
                reply_queue_name TEXT NOT NULL,
                rpc_return_message TEXT NOT NULL,
                rpc_request_queue TEXT NOT NULL,
                persisted_at_utc TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
