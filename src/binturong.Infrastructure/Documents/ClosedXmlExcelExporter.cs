using System.Reflection;
using Application.Abstractions.Documents;
using ClosedXML.Excel;

namespace Infrastructure.Documents;

public sealed class ClosedXmlExcelExporter : IExcelExporter
{
    public byte[] Export<T>(IReadOnlyList<T> rows, string sheetName = "Sheet1")
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);

        var props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        // headers
        for (var c = 0; c < props.Length; c++)
            ws.Cell(1, c + 1).Value = props[c].Name;

        // rows
        for (var r = 0; r < rows.Count; r++)
        {
            for (var c = 0; c < props.Length; c++)
            {
                var val = props[c].GetValue(rows[r]);
                ws.Cell(r + 2, c + 1).Value = val?.ToString() ?? string.Empty;
            }
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
