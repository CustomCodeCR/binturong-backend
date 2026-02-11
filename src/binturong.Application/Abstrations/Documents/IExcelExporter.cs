namespace Application.Abstractions.Documents;

public interface IExcelExporter
{
    byte[] Export<T>(IReadOnlyList<T> rows, string sheetName = "Sheet1");
}
