using System.Text.Json;
using RabbitMQ.Client;

namespace CryptoCheck.Web.Features.CryptoReports;

public sealed class CryptoReportRequestPublisher(IConnection connection, ILogger<CryptoReportRequestPublisher> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Guid> PublishAsync(CryptoReportRequest request, CancellationToken cancellationToken = default)
    {
        var reportId = Guid.NewGuid();
        var message = new CryptoReportRequestMessage(
            reportId,
            DateTime.UtcNow,
            request.Cryptos,
            request.NotifyByEmail,
            request.NotifyBySms,
            request.RecipientEmail,
            request.RecipientPhoneNumber);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            exchange: "request.reports.cryptos.exchange",
            type: ExchangeType.Fanout,
            durable: true,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        var props = new BasicProperties
        {
            ContentType = "application/json",
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: "request.reports.cryptos.exchange",
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogInformation("Solicitacao de relatorio {ReportId} publicada com {CryptoCount} criptos.", reportId, request.Cryptos.Count);
        return reportId;
    }
}

public sealed record CryptoReportRequest(
    IReadOnlyList<string> Cryptos,
    bool NotifyByEmail,
    bool NotifyBySms,
    string? RecipientEmail,
    string? RecipientPhoneNumber);

internal sealed record CryptoReportRequestMessage(
    Guid MessageId,
    DateTime CreatedAt,
    IReadOnlyList<string> Cryptos,
    bool NotifyByEmail,
    bool NotifyBySms,
    string? RecipientEmail,
    string? RecipientPhoneNumber);
