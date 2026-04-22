using ClosedXML.Excel;
using CryptoService.Messages;

namespace CryptoService.Infrastructure.Spreadsheets;

internal static class SpreadsheetLayoutBuilder
{
    public static void BuildWorkbook(XLWorkbook workbook, CreateXlsxCrypto message, CancellationToken cancellationToken)
    {
        foreach (var worksheetDefinition in message.Worksheets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            BuildWorksheet(workbook, worksheetDefinition);
        }
    }

    private static void BuildWorksheet(XLWorkbook workbook, XlsxWorksheetDefinition definition)
    {
        var worksheet = workbook.Worksheets.Add(SanitizeWorksheetName(definition.Name));
        worksheet.Style.Font.FontName = "Segoe UI";
        worksheet.Style.Font.FontSize = 11;
        worksheet.ShowGridLines = false;

        var totalColumns = Math.Max(definition.Columns.Count, 1);
        var currentRow = 1;

        worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Value = definition.Title;
        worksheet.Row(currentRow).Height = 26;
        worksheet.Range(currentRow, 1, currentRow, totalColumns).Style
            .Font.SetBold()
            .Font.SetFontSize(16)
            .Font.SetFontColor(XLColor.White);
        worksheet.Range(currentRow, 1, currentRow, totalColumns).Style.Fill.BackgroundColor = XLColor.FromHtml("#0F4C5C");
        currentRow++;

        if (!string.IsNullOrWhiteSpace(definition.Subtitle))
        {
            worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Value = definition.Subtitle;
            worksheet.Range(currentRow, 1, currentRow, totalColumns).Style
                .Font.SetFontColor(XLColor.FromHtml("#334155"))
                .Font.SetItalic();
            currentRow += 2;
        }
        else
        {
            currentRow++;
        }

        if (definition.SummaryMetrics.Count > 0)
        {
            currentRow = RenderSummaryMetrics(worksheet, definition.SummaryMetrics, currentRow, totalColumns);
            currentRow++;
        }

        int? firstHeaderRow = null;
        var tableIndex = 1;

        foreach (var section in definition.Sections)
        {
            worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Value = section.Title;
            worksheet.Range(currentRow, 1, currentRow, totalColumns).Style
                .Fill.SetBackgroundColor(XLColor.FromHtml("#E0F2FE"))
                .Font.SetBold()
                .Font.SetFontColor(XLColor.FromHtml("#0F172A"));
            currentRow++;

            if (!string.IsNullOrWhiteSpace(section.Subtitle))
            {
                worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Value = section.Subtitle;
                worksheet.Range(currentRow, 1, currentRow, totalColumns).Style
                    .Font.SetFontColor(XLColor.FromHtml("#475569"))
                    .Font.SetItalic();
                currentRow++;
            }

            var headerRow = currentRow;
            firstHeaderRow ??= headerRow;

            for (var columnIndex = 0; columnIndex < definition.Columns.Count; columnIndex++)
            {
                var column = definition.Columns[columnIndex];
                var cell = worksheet.Cell(headerRow, columnIndex + 1);
                cell.Value = column.Header;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D4ED8");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }

            currentRow++;

            if (section.Rows.Count == 0)
            {
                worksheet.Range(currentRow, 1, currentRow, totalColumns).Merge().Value = "Nenhum dado disponivel para esta secao.";
                worksheet.Range(currentRow, 1, currentRow, totalColumns).Style.Font.FontColor = XLColor.FromHtml("#64748B");
                currentRow += 2;
                continue;
            }

            for (var rowIndex = 0; rowIndex < section.Rows.Count; rowIndex++)
            {
                var rowDefinition = section.Rows[rowIndex];
                for (var columnIndex = 0; columnIndex < definition.Columns.Count; columnIndex++)
                {
                    var column = definition.Columns[columnIndex];
                    var cellDefinition = rowDefinition.Cells[columnIndex];
                    var cell = worksheet.Cell(currentRow + rowIndex, columnIndex + 1);
                    ApplyCellValue(cell, cellDefinition);
                    ApplyColumnFormatting(cell, column, cellDefinition);
                }
            }

            var lastDataRow = currentRow + section.Rows.Count - 1;
            var tableRange = worksheet.Range(headerRow, 1, lastDataRow, definition.Columns.Count);
            var table = tableRange.CreateTable($"T{tableIndex:000}_{SanitizeTableName(section.Title)}");
            table.Theme = XLTableTheme.TableStyleMedium9;
            table.SetShowAutoFilter(true);
            table.SetShowTotalsRow(false);
            tableIndex++;

            currentRow = lastDataRow + 2;
        }

        for (var columnIndex = 0; columnIndex < definition.Columns.Count; columnIndex++)
        {
            worksheet.Column(columnIndex + 1).Width = Math.Max(definition.Columns[columnIndex].Width, 12);
        }

        if (firstHeaderRow.HasValue)
        {
            worksheet.SheetView.FreezeRows(firstHeaderRow.Value);
        }

        worksheet.Columns(1, totalColumns).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    private static int RenderSummaryMetrics(
        IXLWorksheet worksheet,
        IReadOnlyList<XlsxSummaryMetricDefinition> metrics,
        int startRow,
        int totalColumns)
    {
        var metricsPerRow = Math.Max(1, totalColumns / 2);
        var currentRow = startRow;
        var metricIndex = 0;

        while (metricIndex < metrics.Count)
        {
            var rowItems = metrics.Skip(metricIndex).Take(metricsPerRow).ToList();
            var currentColumn = 1;

            foreach (var metric in rowItems)
            {
                var labelCell = worksheet.Cell(currentRow, currentColumn);
                labelCell.Value = metric.Label;
                labelCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DBEAFE");
                labelCell.Style.Font.Bold = true;
                labelCell.Style.Font.FontColor = XLColor.FromHtml("#1E3A8A");

                var valueCell = worksheet.Cell(currentRow, currentColumn + 1);
                ApplyCellValue(valueCell, metric.Value);
                valueCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EFF6FF");
                valueCell.Style.Font.Bold = true;
                valueCell.Style.Font.FontColor = XLColor.FromHtml("#0F172A");

                if (metric.Value.ValueType == XlsxCellValueType.Date && !string.IsNullOrWhiteSpace(metric.Value.TextValue))
                {
                    valueCell.Style.DateFormat.Format = metric.Value.TextValue;
                }
                else if (!string.IsNullOrWhiteSpace(metric.Value.TextValue) && metric.Value.ValueType == XlsxCellValueType.Number)
                {
                    valueCell.Style.NumberFormat.Format = metric.Value.TextValue;
                }

                currentColumn += 2;
                if (currentColumn > totalColumns)
                {
                    break;
                }
            }

            currentRow += 2;
            metricIndex += rowItems.Count;
        }

        return currentRow;
    }

    private static void ApplyCellValue(IXLCell cell, XlsxCellDefinition definition)
    {
        if (!string.IsNullOrWhiteSpace(definition.FormulaA1))
        {
            cell.FormulaA1 = definition.FormulaA1;
            return;
        }

        switch (definition.ValueType)
        {
            case XlsxCellValueType.Number:
                cell.Value = definition.NumberValue ?? 0;
                break;
            case XlsxCellValueType.Date:
                cell.Value = definition.DateValue ?? DateTime.UtcNow;
                break;
            case XlsxCellValueType.Boolean:
                cell.Value = definition.BooleanValue ?? false;
                break;
            default:
                cell.Value = definition.TextValue ?? string.Empty;
                break;
        }
    }

    private static void ApplyColumnFormatting(IXLCell cell, XlsxColumnDefinition column, XlsxCellDefinition definition)
    {
        cell.Style.Alignment.Horizontal = column.ValueType switch
        {
            XlsxCellValueType.Number => XLAlignmentHorizontalValues.Right,
            XlsxCellValueType.Date => XLAlignmentHorizontalValues.Center,
            _ => XLAlignmentHorizontalValues.Left
        };

        if (column.ValueType == XlsxCellValueType.Date)
        {
            cell.Style.DateFormat.Format = column.NumberFormat ?? "dd/MM/yyyy";
        }
        else if (column.ValueType == XlsxCellValueType.Number)
        {
            cell.Style.NumberFormat.Format = column.NumberFormat ?? "#,##0.00";
        }

        if (column.Key.Contains("variation", StringComparison.OrdinalIgnoreCase) && definition.NumberValue.HasValue)
        {
            cell.Style.Font.FontColor = definition.NumberValue >= 0
                ? XLColor.FromHtml("#15803D")
                : XLColor.FromHtml("#B91C1C");
            cell.Style.Font.Bold = true;
        }
    }

    private static string SanitizeWorksheetName(string value)
    {
        var invalidCharacters = new[] { '[', ']', '*', '?', '/', '\\', ':' };
        var sanitized = invalidCharacters.Aggregate(value, (current, invalid) => current.Replace(invalid, '-')).Trim();

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "Planilha";
        }

        return sanitized.Length > 31 ? sanitized[..31] : sanitized;
    }

    private static string SanitizeTableName(string value)
    {
        var filtered = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(filtered) ? "Section" : filtered;
    }
}
