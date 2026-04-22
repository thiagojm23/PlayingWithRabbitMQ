using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/CryptoCheck", ([FromBody] IList<string> cryptos) =>
{
    if (cryptos == null || !cryptos.Any())
        return Results.BadRequest("Nenhuma criptomoeda fornecida.");


    return Results.Accepted();
});

app.MapGet("/weatherforecast", () =>
{
    return "";
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record CryptoCheckViewModelReturn(IList<PriceCryptoViewModelReturn> Prices, IList<TradeInfoCryptoViewModelReturn> Trades, Uri SpreadSheet);

public record PriceCryptoViewModelReturn(string CryptoName, decimal CurrentPrice, decimal AvaragePrice, decimal HighestPrice, DateTime DataHighestPrice);

public record TradeInfoCryptoViewModelReturn(string CryptoName);