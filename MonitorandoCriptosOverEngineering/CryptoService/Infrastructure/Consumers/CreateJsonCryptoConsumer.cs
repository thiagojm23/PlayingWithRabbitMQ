using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class CreateJsonCryptoConsumer : BasicRabbitMQConsumer<CreateJsonCryptoPart, CreateJsonCryptoConsumer>
{
    private const string CreatedJsonRoutingKey = "json.cryptos.created";

    private readonly ICreateJsonCryptoAggregationService _aggregationService;
    private readonly IPublisher _publisher;

    public CreateJsonCryptoConsumer(
        IConnection connection,
        ICreateJsonCryptoAggregationService aggregationService,
        IPublisher publisher,
        ILogger<CreateJsonCryptoConsumer> logger)
        : base(connection, logger, "create.json.cryptos.queue")
    {
        _aggregationService = aggregationService;
        _publisher = publisher;
    }

    protected override async Task OnMessageReceived(CreateJsonCryptoPart message)
    {
        var completedReport = await _aggregationService.StoreAndTryCompleteAsync(message);

        if (completedReport is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(completedReport.ReplyQueue))
        {
            await _publisher.PublishMessageAsync(completedReport.ReplyQueue, completedReport);
            return;
        }

        await _publisher.PublishMessageAsync(
            completedReport.ExchangeName,
            CreatedJsonRoutingKey,
            completedReport);
    }
}
