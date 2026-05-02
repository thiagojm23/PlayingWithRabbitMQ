using System.Collections.Concurrent;
using System.Text.Json;
using RabbitMQ.Client;

namespace CryptoCheck.Web.Features.CryptoReports;

public sealed class CryptoReportInbox(IConnection connection, ILogger<CryptoReportInbox> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<PendingCryptoReport>> _waiters = new();
    private readonly ConcurrentDictionary<Guid, PendingCryptoReport> _reports = new();

    public event Func<PendingCryptoReport, Task>? ReportReceived;

    public Task<PendingCryptoReport> WaitForReportAsync(Guid reportId, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (_reports.TryGetValue(reportId, out var existingReport))
        {
            return Task.FromResult(existingReport);
        }

        var waiter = _waiters.GetOrAdd(
            reportId,
            _ => new TaskCompletionSource<PendingCryptoReport>(TaskCreationOptions.RunContinuationsAsynchronously));

        return waiter.Task.WaitAsync(timeout, cancellationToken);
    }

    public async Task ReceiveAsync(PendingCryptoReport report)
    {
        _reports[report.ReportId] = report;

        if (_waiters.TryRemove(report.ReportId, out var waiter))
        {
            waiter.TrySetResult(report);
        }

        if (ReportReceived is not null)
        {
            await ReportReceived.Invoke(report);
        }
    }

    public async Task<bool> CompleteAsync(Guid reportId, string logMessage, CancellationToken cancellationToken = default)
    {
        if (!_reports.TryRemove(reportId, out var report))
        {
            return false;
        }

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        var props = new BasicProperties
        {
            ContentType = "application/json",
            CorrelationId = report.CorrelationId
        };
        var body = JsonSerializer.SerializeToUtf8Bytes(logMessage, JsonOptions);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: report.ReplyTo,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogInformation("Resposta RPC enviada para o relatorio {ReportId}.", reportId);
        return true;
    }
}

public sealed record PendingCryptoReport(
    Guid ReportId,
    int PriceRowCount,
    int TradeRowCount,
    ReportPreviewSummary? PricePreview,
    ReportPreviewSummary? TradePreview,
    JsonElement Price,
    JsonElement Trade,
    JsonElement Spreadsheet,
    string ReplyTo,
    string CorrelationId,
    DateTime ReceivedAtUtc);

public sealed record ReportPreviewSummary(
    string Title,
    string Subtitle,
    int RequestedCryptos,
    int ConsolidatedRowCount,
    IReadOnlyList<ReportPreviewMetric> Metrics,
    IReadOnlyList<ReportPreviewSpotlightItem> Spotlight);

public sealed record ReportPreviewMetric(
    string Label,
    string Value);

public sealed record ReportPreviewSpotlightItem(
    string Label,
    string PrimaryValue,
    string? SecondaryValue);
