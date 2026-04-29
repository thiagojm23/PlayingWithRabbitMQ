using CryptoCheck.Web.Components;
using CryptoCheck.Web.Features.CryptoReports;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory();
    builder.Configuration.GetSection("RabbitMQ").Bind(factory);
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
builder.Services.AddSingleton<CryptoReportInbox>();
builder.Services.AddSingleton<CryptoReportRequestPublisher>();
builder.Services.AddHostedService<RabbitMqCryptoReportConsumer>();
builder.Services.AddHttpClient<BinanceCryptoCatalogClient>(client =>
{
    client.BaseAddress = new Uri("https://api.binance.com/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
