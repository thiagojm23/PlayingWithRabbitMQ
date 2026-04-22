namespace CryptoService.Infrastructure;

internal static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMqConsumers(this IServiceCollection services)
    {
        var consumerContractType = typeof(Consumers.IRabbitMqConsumer);
        var consumerTypes = consumerContractType.Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && consumerContractType.IsAssignableFrom(type));

        foreach (var consumerType in consumerTypes)
        {
            services.AddSingleton(consumerContractType, consumerType);
        }

        services.AddHostedService<RabbitMqConsumersHostedService>();

        return services;
    }

    public static IServiceCollection AddRabbitMqTopology(
        this IServiceCollection services,
        Action<RabbitMqTopologyOptions> configure)
    {
        var options = new RabbitMqTopologyOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddHostedService<RabbitMqTopologySetup>();

        return services;
    }
}
