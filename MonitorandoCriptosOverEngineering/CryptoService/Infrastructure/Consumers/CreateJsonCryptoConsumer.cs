using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class CreateJsonCryptoConsumer : BasicRabbitMQConsumer<CreateJsonCryptoPart, CreateJsonCryptoConsumer>
{
    private readonly ICreateJsonCryptoAggregationService _aggregationService;
    private readonly ILogger<CreateJsonCryptoConsumer> _logger;
    private readonly IPublisher _publisher;
    private readonly IRpcResponseMessagePersistenceService _rpcResponseMessagePersistenceService;

    public CreateJsonCryptoConsumer(
        IConnection connection,
        ICreateJsonCryptoAggregationService aggregationService,
        ILogger<CreateJsonCryptoConsumer> logger,
        IPublisher publisher,
        IRpcResponseMessagePersistenceService rpcResponseMessagePersistenceService)
        : base(connection, logger, "create.json.cryptos.queue")
    {
        _aggregationService = aggregationService;
        _logger = logger;
        _publisher = publisher;
        _rpcResponseMessagePersistenceService = rpcResponseMessagePersistenceService;
    }

    protected override async Task OnMessageReceived(CreateJsonCryptoPart message)
    {
        var completedReport = await _aggregationService.StoreAndTryCompleteAsync(message);

        if (completedReport is null)
        {
            return;
        }

        var responseRPC = await _publisher.PublishRpcMessageAsync<CreatedJsonCryptoReport, string>("receive.json.cryptos.queue", completedReport, "CreateJsonCryptoReport", new TimeSpan(0, 0, 10));

        await _rpcResponseMessagePersistenceService.PersistAsync(responseRPC);

        _logger.LogInformation("JSON consolidado {ReportId} pronto para envio via RPC.", completedReport.ReportId);
    }
}
