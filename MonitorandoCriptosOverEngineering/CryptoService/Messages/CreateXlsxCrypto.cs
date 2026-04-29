using System.Text.Json;

namespace CryptoService.Messages;

internal sealed class CreateXlsxCrypto : BaseMessage
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override string ExchangeName => "create.docs.cryptos.direct";

    public string WorkbookTitle { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SourceMessageId { get; set; }
    public bool NotifyByEmail { get; set; }
    public bool NotifyBySms { get; set; }
    public string? RecipientEmail { get; set; }
    public string? RecipientPhoneNumber { get; set; }
    public List<XlsxWorksheetDefinition> Worksheets { get; set; } = [];

    public CreateJsonCryptoPart CreateJsonPart(string dataType)
    {
        return new CreateJsonCryptoPart
        {
            ReportId = SourceMessageId == Guid.Empty ? MessageId : SourceMessageId,
            DataType = dataType,
            Data = JsonSerializer.SerializeToElement(this, JsonOptions)
        };
    }

    public CreateJsonCryptoPart CreateSpreadsheetJsonPart(byte[] content)
    {
        var generatedMessage = new XlsxCryptoGenerated
        {
            SourceCreateMessageId = MessageId,
            WorkbookTitle = WorkbookTitle,
            FileName = FileName,
            Content = content
        };

        return new CreateJsonCryptoPart
        {
            ReportId = SourceMessageId == Guid.Empty ? MessageId : SourceMessageId,
            DataType = "Spreadsheet",
            Data = JsonSerializer.SerializeToElement(generatedMessage, JsonOptions)
        };
    }
}

internal sealed class XlsxWorksheetDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public List<XlsxSummaryMetricDefinition> SummaryMetrics { get; set; } = [];
    public List<XlsxColumnDefinition> Columns { get; set; } = [];
    public List<XlsxSectionDefinition> Sections { get; set; } = [];
}

internal sealed class XlsxSummaryMetricDefinition
{
    public string Label { get; set; } = string.Empty;
    public XlsxCellDefinition Value { get; set; } = new();
}

internal sealed class XlsxColumnDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public XlsxCellValueType ValueType { get; set; } = XlsxCellValueType.Text;
    public string? NumberFormat { get; set; }
    public int Width { get; set; } = 18;
}

internal sealed class XlsxSectionDefinition
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public List<XlsxRowDefinition> Rows { get; set; } = [];
}

internal sealed class XlsxRowDefinition
{
    public List<XlsxCellDefinition> Cells { get; set; } = [];
}

internal sealed class XlsxCellDefinition
{
    public XlsxCellValueType ValueType { get; set; } = XlsxCellValueType.Text;
    public string? TextValue { get; set; }
    public decimal? NumberValue { get; set; }
    public DateTime? DateValue { get; set; }
    public bool? BooleanValue { get; set; }
    public string? FormulaA1 { get; set; }
}

internal enum XlsxCellValueType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3
}
