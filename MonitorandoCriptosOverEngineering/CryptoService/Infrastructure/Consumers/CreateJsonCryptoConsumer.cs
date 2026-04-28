using CryptoService.Abstractions;
using CryptoService.Messages;
using RabbitMQ.Client;

namespace CryptoService.Infrastructure.Consumers;

internal sealed class CreateJsonCryptoConsumer : BasicRabbitMQConsumer<CreateJsonCryptoPart, CreateJsonCryptoConsumer>
{
    private readonly ICreateJsonCryptoAggregationService _aggregationService;
    private readonly ILogger<CreateJsonCryptoConsumer> _logger;

    public CreateJsonCryptoConsumer(
        IConnection connection,
        ICreateJsonCryptoAggregationService aggregationService,
        ILogger<CreateJsonCryptoConsumer> logger)
        : base(connection, logger, "create.json.cryptos.queue")
    {
        _aggregationService = aggregationService;
        _logger = logger;
    }

    protected override async Task OnMessageReceived(CreateJsonCryptoPart message)
    {
        var completedReport = await _aggregationService.StoreAndTryCompleteAsync(message);

        if (completedReport is null)
        {
            return;
        }

        _logger.LogInformation("JSON consolidado {ReportId} pronto para envio via RPC.", completedReport.ReportId);
    }
}
