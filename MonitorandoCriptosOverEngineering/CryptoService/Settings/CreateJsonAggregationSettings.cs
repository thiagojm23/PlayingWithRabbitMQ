namespace CryptoService.Settings;

internal sealed class CreateJsonAggregationSettings
{
    public const string SectionName = "CreateJsonAggregation";

    public int ExpirationMinutes { get; set; } = 15;
}
