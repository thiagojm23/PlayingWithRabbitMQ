using CryptoService.Abstractions;
using CryptoService.Infrastructure;
using CryptoService.Infrastructure.Clients;
using CryptoService.Services;
using CryptoService.Services.ComunicationServices;
using CryptoService.Settings;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory();
    builder.Configuration.GetSection("RabbitMQ").Bind(factory);
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddSingleton<IPublisher, Publisher>();
builder.Services.AddHttpClient<IBinanceClient, BinanceClient>(client =>
{
    client.BaseAddress = new Uri("https://api.binance.com/");
});
builder.Services.Configure<GmailSmtpSettings>(builder.Configuration.GetSection(GmailSmtpSettings.SectionName));
builder.Services.Configure<SmsSandboxSettings>(builder.Configuration.GetSection(SmsSandboxSettings.SectionName));
builder.Services.Configure<NotificationSqliteSettings>(builder.Configuration.GetSection(NotificationSqliteSettings.SectionName));
builder.Services.AddSingleton<IAnalyzeCriptoService, AnalyzeCrypyoService>();
builder.Services.AddSingleton<IXlsxCryptoGeneratorService, XlsxCryptoGeneratorService>();
builder.Services.AddSingleton<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddSingleton<ISmsNotificationService, SmsNotificationService>();
builder.Services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
builder.Services.AddSingleton<INotificationLogPersistenceService, SqliteNotificationLogPersistenceService>();

builder.Services.AddRabbitMqTopology(topology => topology
    //Ponta de entrada para solicitações de relatórios.
    .WithExchange("request.reports.cryptos.exchange", ExchangeType.Fanout)
    .WithQueue("reports.cryptos.prices.queue")
    .WithBinding("request.reports.cryptos.exchange", "reports.cryptos.prices.queue", "")
    .WithQueue("reports.cryptos.trades.queue")
    .WithBinding("request.reports.cryptos.exchange", "reports.cryptos.trades.queue", "")

    //Exchange para geração de planilhas XLSX.
    .WithExchange("create.docs.cryptos.direct", ExchangeType.Direct)
    .WithQueue("create.xlsx.cryptos.queue")
    .WithBinding("create.docs.cryptos.direct", "create.xlsx.cryptos.queue", "xlsx.cryptos")
    //.WithQueue("created.xlsx.cryptos.queue")
    //.WithBinding("created.docs.cryptos.direct", "created.xlsx.cryptos.queue", "xlsx.cryptos.generated")

    //Fila para montar JSON no front
    //Apenas para praticar - vai ser uma point-to-point
    .WithQueue("create.json.cryptos.queue")

    //Exchange para envio de notificações
    .WithExchange("notify.users.cryptos.topic", ExchangeType.Topic)
    .WithQueue("notify.email.cryptos.queue")
    .WithQueue("notify.sms.cryptos.queue")
    .WithQueue("notify.save.logs.cryptos.queue")
    .WithBinding("notify.users.cryptos.topic", "notify.email.cryptos.queue", "notify.email.#")
    .WithBinding("notify.users.cryptos.topic", "notify.sms.cryptos.queue", "notify.sms.#")
    .WithBinding("notify.users.cryptos.topic", "notify.save.logs.cryptos.queue", "notify.#")
);

builder.Services.AddRabbitMqConsumers();

var host = builder.Build();
host.Run();
