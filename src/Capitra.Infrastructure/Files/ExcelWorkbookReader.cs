using System.Globalization;
using Capitra.Application.Abstractions;
using ClosedXML.Excel;

namespace Capitra.Infrastructure.Files;

/// <summary><see cref="IWorkbookReader"/> on ClosedXML (the .NET side of APEX_DATA_PARSER for .xlsx files).</summary>
public sealed class ExcelWorkbookReader : IWorkbookReader
{
    public IReadOnlyDictionary<string, IReadOnlyList<WorkbookRow>> Read(Stream content)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(content);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new InvalidDataException("The file is not a valid Excel workbook (.xlsx).", ex);
        }

        using (workbook)
        {
            var sheets = new Dictionary<string, IReadOnlyList<WorkbookRow>>(StringComparer.OrdinalIgnoreCase);
            foreach (var sheet in workbook.Worksheets)
                sheets.TryAdd(sheet.Name.Trim().ToUpperInvariant(), ReadRows(sheet));
            return sheets;
        }
    }

    private static List<WorkbookRow> ReadRows(IXLWorksheet sheet)
    {
        var rows = new List<WorkbookRow>();
        var used = sheet.RangeUsed();
        if (used is null)
            return rows;

        var lastColumn = used.LastColumn().ColumnNumber();
        foreach (var row in sheet.RowsUsed().Where(r => r.RowNumber() > 1))
        {
            var cells = new string?[lastColumn];
            for (var c = 1; c <= lastColumn; c++)
                cells[c - 1] = TextOf(row.Cell(c));

            if (cells.Any(x => x is not null))
                rows.Add(new WorkbookRow(row.RowNumber(), cells));
        }

        return rows;
    }

    private static string? TextOf(IXLCell cell)
    {
        var value = cell.Value;
        string? text = value.Type switch
        {
            XLDataType.Blank => null,
            XLDataType.DateTime => value.GetDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            XLDataType.Number => value.GetNumber().ToString(CultureInfo.InvariantCulture),
            XLDataType.Boolean => value.GetBoolean() ? "Y" : "N",
            _ => cell.GetFormattedString()
        };
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}
