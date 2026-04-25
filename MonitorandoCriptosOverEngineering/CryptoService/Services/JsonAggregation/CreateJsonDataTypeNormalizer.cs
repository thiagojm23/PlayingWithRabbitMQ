namespace CryptoService.Services.JsonAggregation;

internal static class CreateJsonDataTypeNormalizer
{
    public static bool TryNormalize(string value, out CreateJsonDataType dataType)
    {
        switch (Normalize(value))
        {
            case "price":
            case "prices":
            case "preco":
            case "precos":
                dataType = CreateJsonDataType.Price;
                return true;
            case "trade":
            case "trades":
                dataType = CreateJsonDataType.Trade;
                return true;
            case "spreadsheet":
            case "xlsx":
            case "planilha":
            case "sheet":
                dataType = CreateJsonDataType.Spreadsheet;
                return true;
            default:
                dataType = default;
                return false;
        }
    }

    private static string Normalize(string value) =>
        value.Trim().ToLowerInvariant();
}
