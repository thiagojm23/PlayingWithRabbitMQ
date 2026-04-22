using ClosedXML.Excel;
using CryptoService.Abstractions;
using CryptoService.Infrastructure.Spreadsheets;
using CryptoService.Messages;

namespace CryptoService.Services;

internal sealed class XlsxCryptoGeneratorService : IXlsxCryptoGeneratorService
{
    public Task<byte[]> GenerateAsync(CreateXlsxCrypto message, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        workbook.Properties.Author = "CryptoService";
        workbook.Properties.Title = message.WorkbookTitle;
        workbook.Properties.Subject = message.Description ?? message.ReportType;

        SpreadsheetLayoutBuilder.BuildWorkbook(workbook, message, cancellationToken);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
