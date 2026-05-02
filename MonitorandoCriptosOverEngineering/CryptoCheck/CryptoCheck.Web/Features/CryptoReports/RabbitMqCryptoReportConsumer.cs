using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CryptoCheck.Web.Features.CryptoReports;

public sealed class RabbitMqCryptoReportConsumer(
    IConnection connection,
    CryptoReportInbox inbox,
    ILogger<RabbitMqCryptoReportConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(
            queue: "receive.json.cryptos.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: "receive.json.cryptos.queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var message = JsonSerializer.Deserialize<CreatedJsonCryptoReportMessage>(ea.Body.Span, JsonOptions);

            if (message is null || string.IsNullOrWhiteSpace(ea.BasicProperties.ReplyTo) || string.IsNullOrWhiteSpace(ea.BasicProperties.CorrelationId))
            {
                await _channel.BasicRejectAsync(ea.DeliveryTag, requeue: false);
                return;
            }

            await inbox.ReceiveAsync(new PendingCryptoReport(
                message.ReportId,
                message.PriceRowCount,
                message.TradeRowCount,
                message.PricePreview,
                message.TradePreview,
                message.Price.Clone(),
                message.Trade.Clone(),
                message.Spreadsheet.Clone(),
                ea.BasicProperties.ReplyTo,
                ea.BasicProperties.CorrelationId,
                DateTime.UtcNow));

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consumir relatorio RPC do CryptoService.");
            await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    private sealed record CreatedJsonCryptoReportMessage(
        Guid ReportId,
        int PriceRowCount,
        int TradeRowCount,
        ReportPreviewSummary? PricePreview,
        ReportPreviewSummary? TradePreview,
        JsonElement Price,
        JsonElement Trade,
        JsonElement Spreadsheet);
}
