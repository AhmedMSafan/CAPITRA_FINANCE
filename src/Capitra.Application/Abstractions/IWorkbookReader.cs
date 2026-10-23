namespace Capitra.Application.Abstractions;

/// <summary>One data row of a worksheet: its row number in Excel and the cell texts, left to right.</summary>
public sealed record WorkbookRow(int RowNumber, IReadOnlyList<string?> Cells)
{
    public string? this[int column] => column < Cells.Count ? Cells[column] : null;
}

/// <summary>Reads .xlsx workbooks. Dates come back as yyyy-MM-dd and numbers in invariant culture.</summary>
public interface IWorkbookReader
{
    /// <summary>
    /// The worksheets of the workbook by name (upper case), each with its rows after the header row.
    /// Rows with no value in any cell are left out. Throws <see cref="InvalidDataException"/> when the file is not a workbook.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<WorkbookRow>> Read(Stream content);
}
