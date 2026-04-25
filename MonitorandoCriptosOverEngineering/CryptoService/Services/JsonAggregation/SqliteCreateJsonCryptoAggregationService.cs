using System.Text.Json;
using CryptoService.Abstractions;
using CryptoService.Messages;
using CryptoService.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace CryptoService.Services.JsonAggregation;

internal sealed class SqliteCreateJsonCryptoAggregationService(
    ISqliteConnectionFactory sqliteConnectionFactory,
    IOptions<CreateJsonAggregationSettings> settings,
    ILogger<SqliteCreateJsonCryptoAggregationService> logger) : ICreateJsonCryptoAggregationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(Math.Max(1, settings.Value.ExpirationMinutes));

    public async Task<CreatedJsonCryptoReport?> StoreAndTryCompleteAsync(
        CreateJsonCryptoPart message,
        CancellationToken cancellationToken = default)
    {
        if (message.ReportId == Guid.Empty)
        {
            throw new InvalidOperationException("A parte recebida para criacao do JSON nao possui Guid valido.");
        }

        if (!CreateJsonDataTypeNormalizer.TryNormalize(message.DataType, out var dataType))
        {
            throw new InvalidOperationException($"TipoDado invalido para criacao do JSON: {message.DataType}.");
        }

        var payload = SerializePayload(message);
        await using var connection = await sqliteConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);
        await DeleteExpiredAsync(connection, cancellationToken);

        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await UpsertPartAsync(connection, transaction, message, dataType, payload, cancellationToken);

        var parts = await LoadPartsAsync(connection, transaction, message.ReportId, cancellationToken);

        if (parts.Count < 3)
        {
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation(
                "Parte {DataType} armazenada para o JSON {ReportId}. Partes recebidas: {PartsCount}/3.",
                dataType,
                message.ReportId,
                parts.Count);
            return null;
        }

        var completed = CreateCompletedReport(message.ReportId, parts);
        await DeleteReportPartsAsync(connection, transaction, message.ReportId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("JSON consolidado montado para o Guid {ReportId}.", message.ReportId);
        return completed;
    }

    private static string SerializePayload(CreateJsonCryptoPart message)
    {
        var payload = message.ResolvePayload();

        return payload is null
            ? JsonSerializer.Serialize(message, JsonOptions)
            : JsonSerializer.Serialize(payload.Value, JsonOptions);
    }

    private static CreatedJsonCryptoReport CreateCompletedReport(
        Guid reportId,
        IReadOnlyDictionary<CreateJsonDataType, CreateJsonPartState> parts)
    {
        var replyPart = parts.Values.FirstOrDefault(part => !string.IsNullOrWhiteSpace(part.ReplyQueue));

        return new CreatedJsonCryptoReport
        {
            ReportId = reportId,
            RpcCorrelationId = replyPart?.RpcCorrelationId,
            ReplyQueue = replyPart?.ReplyQueue,
            Price = ParsePayload(parts[CreateJsonDataType.Price].PayloadJson),
            Trade = ParsePayload(parts[CreateJsonDataType.Trade].PayloadJson),
            Spreadsheet = ParsePayload(parts[CreateJsonDataType.Spreadsheet].PayloadJson)
        };
    }

    private static JsonElement ParsePayload(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS create_json_crypto_parts
            (
                report_id TEXT NOT NULL,
                data_type TEXT NOT NULL,
                payload_json TEXT NOT NULL,
                reply_queue TEXT NULL,
                rpc_correlation_id TEXT NULL,
                received_at_utc TEXT NOT NULL,
                PRIMARY KEY (report_id, data_type)
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task DeleteExpiredAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            DELETE FROM create_json_crypto_parts
            WHERE received_at_utc < $expiresBeforeUtc;
            """;
        command.Parameters.AddWithValue("$expiresBeforeUtc", DateTime.UtcNow.Subtract(_expiration));

        var deleted = await command.ExecuteNonQueryAsync(cancellationToken);

        if (deleted > 0)
        {
            logger.LogInformation("{DeletedCount} partes expiradas de JSON foram removidas.", deleted);
        }
    }

    private static async Task UpsertPartAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        CreateJsonCryptoPart message,
        CreateJsonDataType dataType,
        string payload,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            INSERT INTO create_json_crypto_parts
            (
                report_id,
                data_type,
                payload_json,
                reply_queue,
                rpc_correlation_id,
                received_at_utc
            )
            VALUES
            (
                $reportId,
                $dataType,
                $payloadJson,
                $replyQueue,
                $rpcCorrelationId,
                $receivedAtUtc
            )
            ON CONFLICT(report_id, data_type)
            DO UPDATE SET
                payload_json = excluded.payload_json,
                reply_queue = COALESCE(excluded.reply_queue, create_json_crypto_parts.reply_queue),
                rpc_correlation_id = COALESCE(excluded.rpc_correlation_id, create_json_crypto_parts.rpc_correlation_id),
                received_at_utc = excluded.received_at_utc;
            """;
        command.Parameters.AddWithValue("$reportId", message.ReportId.ToString("D"));
        command.Parameters.AddWithValue("$dataType", dataType.ToString());
        command.Parameters.AddWithValue("$payloadJson", payload);
        command.Parameters.AddWithValue("$replyQueue", (object?)message.ReplyQueue ?? DBNull.Value);
        command.Parameters.AddWithValue("$rpcCorrelationId", (object?)message.RpcCorrelationId ?? DBNull.Value);
        command.Parameters.AddWithValue("$receivedAtUtc", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Dictionary<CreateJsonDataType, CreateJsonPartState>> LoadPartsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid reportId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            SELECT data_type, payload_json, reply_queue, rpc_correlation_id
            FROM create_json_crypto_parts
            WHERE report_id = $reportId;
            """;
        command.Parameters.AddWithValue("$reportId", reportId.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var parts = new Dictionary<CreateJsonDataType, CreateJsonPartState>();

        while (await reader.ReadAsync(cancellationToken))
        {
            if (Enum.TryParse<CreateJsonDataType>(reader.GetString(0), out var dataType))
            {
                parts[dataType] = new CreateJsonPartState(
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3));
            }
        }

        return parts;
    }

    private static async Task DeleteReportPartsAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid reportId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            """
            DELETE FROM create_json_crypto_parts
            WHERE report_id = $reportId;
            """;
        command.Parameters.AddWithValue("$reportId", reportId.ToString("D"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

internal sealed record CreateJsonPartState(
    string PayloadJson,
    string? ReplyQueue,
    string? RpcCorrelationId);
