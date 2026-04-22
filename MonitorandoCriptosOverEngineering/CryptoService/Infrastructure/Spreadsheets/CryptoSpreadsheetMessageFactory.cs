using CryptoService.Messages;

namespace CryptoService.Infrastructure.Spreadsheets;

internal static class CryptoSpreadsheetMessageFactory
{
    public static CreateXlsxCrypto CreatePriceWorkbookMessage(
        IReadOnlyCollection<string> cryptos,
        IReadOnlyCollection<PriceSpreadsheetRow> priceRows)
    {
        var generatedAt = DateTime.UtcNow;
        var orderedRows = priceRows
            .OrderByDescending(row => row.Day)
            .ThenBy(row => row.Symbol)
            .ToList();

        return new CreateXlsxCrypto
        {
            ReportType = "prices",
            WorkbookTitle = "Relatorio de precos de criptomoedas",
            FileName = $"relatorio-precos-criptos-{generatedAt:yyyyMMdd-HHmmss}.xlsx",
            Description = "Consolidado dos ultimos 7 candles diarios (1d) retornados pela Binance. Datas em UTC.",
            Worksheets =
            [
                new XlsxWorksheetDefinition
                {
                    Name = "Resumo Geral",
                    Title = "Resumo geral de precos",
                    Subtitle = "Visao consolidada dos ultimos 7 dias por cripto.",
                    SummaryMetrics =
                    [
                        CreateMetric("Criptos analisadas", cryptos.Count),
                        CreateMetric("Linhas no consolidado", orderedRows.Count),
                        CreateMetric("Maior maxima", orderedRows.Count == 0 ? 0 : orderedRows.Max(row => row.High), "#,##0.00000000"),
                        CreateMetric("Menor minima", orderedRows.Count == 0 ? 0 : orderedRows.Min(row => row.Low), "#,##0.00000000"),
                        CreateMetric("Media do fechamento", orderedRows.Count == 0 ? 0 : orderedRows.Average(row => row.Close), "#,##0.00000000"),
                        CreateMetric("Volume total", orderedRows.Sum(row => row.Volume), "#,##0.00000000")
                    ],
                    Columns = CreatePriceColumns(),
                    Sections =
                    [
                        new XlsxSectionDefinition
                        {
                            Title = "Todos os dias consolidados",
                            Subtitle = "Tabela unica para leitura rapida e filtros.",
                            Rows = orderedRows.Select(ToPriceRowDefinition).ToList()
                        }
                    ]
                },
                new XlsxWorksheetDefinition
                {
                    Name = "Por Dia",
                    Title = "Precos separados por dia",
                    Subtitle = "Cada secao agrupa as criptomoedas por data.",
                    Columns = CreatePriceColumns(),
                    Sections = orderedRows
                        .GroupBy(row => row.Day)
                        .OrderByDescending(group => group.Key)
                        .Select(group => new XlsxSectionDefinition
                        {
                            Title = $"Dia {group.Key:dd/MM/yyyy}",
                            Subtitle = "Comparativo de precos por cripto nesta data.",
                            Rows = group.OrderBy(row => row.Symbol).Select(ToPriceRowDefinition).ToList()
                        })
                        .ToList()
                }
            ]
        };
    }

    public static CreateXlsxCrypto CreateTradeWorkbookMessage(
        IReadOnlyCollection<string> cryptos,
        IReadOnlyCollection<TradeSpreadsheetRow> tradeRows,
        int tradesPerCrypto)
    {
        var generatedAt = DateTime.UtcNow;
        var orderedRows = tradeRows
            .OrderByDescending(row => row.TradeTimeUtc)
            .ThenBy(row => row.Symbol)
            .ToList();

        return new CreateXlsxCrypto
        {
            ReportType = "trades",
            WorkbookTitle = "Relatorio de trades de criptomoedas",
            FileName = $"relatorio-trades-criptos-{generatedAt:yyyyMMdd-HHmmss}.xlsx",
            Description = "Consolidado dos trades recentes retornados pela Binance. Horarios em UTC.",
            Worksheets =
            [
                new XlsxWorksheetDefinition
                {
                    Name = "Resumo Geral",
                    Title = "Resumo geral de trades",
                    Subtitle = $"Visao consolidada dos ultimos {tradesPerCrypto} trades por cripto.",
                    SummaryMetrics =
                    [
                        CreateMetric("Criptos analisadas", cryptos.Count),
                        CreateMetric("Trades consolidados", orderedRows.Count),
                        CreateMetric("Preco medio", orderedRows.Count == 0 ? 0 : orderedRows.Average(row => row.Price), "#,##0.00000000"),
                        CreateMetric("Quantidade total", orderedRows.Sum(row => row.Quantity), "#,##0.00000000"),
                        CreateMetric("Valor cotado total", orderedRows.Sum(row => row.QuoteQuantity), "#,##0.00000000"),
                        CreateMetric("Ultimo horario", orderedRows.Count == 0 ? generatedAt : orderedRows.Max(row => row.TradeTimeUtc), "dd/MM/yyyy HH:mm:ss")
                    ],
                    Columns = CreateTradeColumns(),
                    Sections =
                    [
                        new XlsxSectionDefinition
                        {
                            Title = "Todos os trades consolidados",
                            Subtitle = "Tabela unica para analise geral e filtros.",
                            Rows = orderedRows.Select(ToTradeRowDefinition).ToList()
                        }
                    ]
                },
                new XlsxWorksheetDefinition
                {
                    Name = "Lotes 50 Trades",
                    Title = "Trades separados em lotes de 50",
                    Subtitle = "Cada secao organiza 50 trades por cripto para facilitar leitura operacional.",
                    Columns = CreateTradeColumns(),
                    Sections = BuildTradeSections(orderedRows)
                }
            ]
        };
    }

    private static List<XlsxSectionDefinition> BuildTradeSections(IReadOnlyCollection<TradeSpreadsheetRow> rows)
    {
        var sections = new List<XlsxSectionDefinition>();

        foreach (var symbolGroup in rows.GroupBy(row => row.Symbol).OrderBy(group => group.Key))
        {
            var trades = symbolGroup
                .OrderByDescending(row => row.TradeTimeUtc)
                .ToList();

            for (var index = 0; index < trades.Count; index += 50)
            {
                var batch = trades.Skip(index).Take(50).ToList();
                var batchNumber = (index / 50) + 1;

                sections.Add(new XlsxSectionDefinition
                {
                    Title = $"{symbolGroup.Key} - lote {batchNumber:00}",
                    Subtitle = $"Trades {index + 1} a {index + batch.Count}.",
                    Rows = batch.Select(ToTradeRowDefinition).ToList()
                });
            }
        }

        return sections;
    }

    private static List<XlsxColumnDefinition> CreatePriceColumns()
    {
        return
        [
            new() { Key = "Symbol", Header = "Cripto", Width = 14 },
            new() { Key = "Day", Header = "Dia", ValueType = XlsxCellValueType.Date, NumberFormat = "dd/MM/yyyy", Width = 14 },
            new() { Key = "Open", Header = "Abertura", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 16 },
            new() { Key = "High", Header = "Maxima", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 16 },
            new() { Key = "Low", Header = "Minima", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 16 },
            new() { Key = "Close", Header = "Fechamento", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 16 },
            new() { Key = "VariationPct", Header = "Variacao %", ValueType = XlsxCellValueType.Number, NumberFormat = "0.00%", Width = 14 },
            new() { Key = "Volume", Header = "Volume", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 18 }
        ];
    }

    private static List<XlsxColumnDefinition> CreateTradeColumns()
    {
        return
        [
            new() { Key = "Symbol", Header = "Cripto", Width = 14 },
            new() { Key = "TradeId", Header = "Trade ID", ValueType = XlsxCellValueType.Number, NumberFormat = "0", Width = 14 },
            new() { Key = "TradeTimeUtc", Header = "Horario UTC", ValueType = XlsxCellValueType.Date, NumberFormat = "dd/MM/yyyy HH:mm:ss", Width = 22 },
            new() { Key = "Price", Header = "Preco", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 16 },
            new() { Key = "Quantity", Header = "Quantidade", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 16 },
            new() { Key = "QuoteQuantity", Header = "Valor cotado", ValueType = XlsxCellValueType.Number, NumberFormat = "#,##0.00000000", Width = 18 },
            new() { Key = "AggressorSide", Header = "Agressor", Width = 18 },
            new() { Key = "BestMatch", Header = "Melhor match", Width = 14 }
        ];
    }

    private static XlsxRowDefinition ToPriceRowDefinition(PriceSpreadsheetRow row)
    {
        return new()
        {
            Cells =
            [
                TextCell(row.Symbol),
                DateCell(row.Day),
                NumberCell(row.Open),
                NumberCell(row.High),
                NumberCell(row.Low),
                NumberCell(row.Close),
                NumberCell(row.VariationPct),
                NumberCell(row.Volume)
            ]
        };
    }

    private static XlsxRowDefinition ToTradeRowDefinition(TradeSpreadsheetRow row)
    {
        return new()
        {
            Cells =
            [
                TextCell(row.Symbol),
                NumberCell(row.TradeId),
                DateCell(row.TradeTimeUtc),
                NumberCell(row.Price),
                NumberCell(row.Quantity),
                NumberCell(row.QuoteQuantity),
                TextCell(row.AggressorSide),
                TextCell(row.BestMatch)
            ]
        };
    }

    private static XlsxSummaryMetricDefinition CreateMetric(string label, decimal value, string? numberFormat = null)
    {
        return new()
        {
            Label = label,
            Value = NumberCell(value, numberFormat)
        };
    }

    private static XlsxSummaryMetricDefinition CreateMetric(string label, int value, string? numberFormat = null)
    {
        return new()
        {
            Label = label,
            Value = NumberCell(value, numberFormat ?? "0")
        };
    }

    private static XlsxSummaryMetricDefinition CreateMetric(string label, DateTime value, string? numberFormat = null)
    {
        return new()
        {
            Label = label,
            Value = new XlsxCellDefinition
            {
                ValueType = XlsxCellValueType.Date,
                DateValue = value,
                TextValue = numberFormat
            }
        };
    }

    private static XlsxCellDefinition TextCell(string value)
    {
        return new()
        {
            ValueType = XlsxCellValueType.Text,
            TextValue = value
        };
    }

    private static XlsxCellDefinition DateCell(DateTime value)
    {
        return new()
        {
            ValueType = XlsxCellValueType.Date,
            DateValue = value
        };
    }

    private static XlsxCellDefinition NumberCell(decimal value, string? format = null)
    {
        return new()
        {
            ValueType = XlsxCellValueType.Number,
            NumberValue = value,
            TextValue = format
        };
    }
}

internal sealed record PriceSpreadsheetRow(
    string Symbol,
    DateTime Day,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    decimal VariationPct);

internal sealed record TradeSpreadsheetRow(
    string Symbol,
    long TradeId,
    DateTime TradeTimeUtc,
    decimal Price,
    decimal Quantity,
    decimal QuoteQuantity,
    string AggressorSide,
    string BestMatch);
