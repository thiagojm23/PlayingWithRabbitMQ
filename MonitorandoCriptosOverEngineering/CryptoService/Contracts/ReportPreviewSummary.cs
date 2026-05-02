namespace CryptoService.Contracts;

internal sealed class ReportPreviewSummary
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public int RequestedCryptos { get; init; }
    public int ConsolidatedRowCount { get; init; }
    public List<ReportPreviewMetric> Metrics { get; init; } = [];
    public List<ReportPreviewSpotlightItem> Spotlight { get; init; } = [];
}

internal sealed class ReportPreviewMetric
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

internal sealed class ReportPreviewSpotlightItem
{
    public string Label { get; init; } = string.Empty;
    public string PrimaryValue { get; init; } = string.Empty;
    public string? SecondaryValue { get; init; }
}
