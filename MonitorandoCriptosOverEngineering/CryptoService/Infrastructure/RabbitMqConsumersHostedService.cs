using CryptoService.Abstractions;

namespace CryptoService.Infrastructure;

internal sealed class RabbitMqConsumersHostedService(
    IEnumerable<IRabbitMqConsumer> consumers,
    ILogger<RabbitMqConsumersHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerList = consumers.ToArray();

        if (consumerList.Length == 0)
        {
            logger.LogWarning("Nenhum consumer RabbitMQ foi encontrado para inicializar.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        foreach (var consumer in consumerList)
        {
            stoppingToken.ThrowIfCancellationRequested();

            logger.LogInformation("Inicializando consumer {ConsumerName}.", consumer.GetType().Name);
            await consumer.StartAsync(stoppingToken);
        }

        logger.LogInformation("Consumers RabbitMQ inicializados: {ConsumersCount}.", consumerList.Length);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
