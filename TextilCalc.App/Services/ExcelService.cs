using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TextilCalc.App.Models;
using SpreadsheetBorder = DocumentFormat.OpenXml.Spreadsheet.Border;
using SpreadsheetCell = DocumentFormat.OpenXml.Spreadsheet.Cell;
using SpreadsheetFont = DocumentFormat.OpenXml.Spreadsheet.Font;

namespace TextilCalc.App.Services;

public sealed class ExcelService : IExcelService
{
    public Task<Stream> ExportAsync(
        IReadOnlyList<Tela> telas,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telas);
        if (telas.Count == 0)
        {
            throw new InvalidOperationException("No hay telas para exportar.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(
                new Columns(
                    new Column { Min = 1, Max = 1, Width = 30, CustomWidth = true },
                    new Column { Min = 2, Max = 3, Width = 14, CustomWidth = true }),
                sheetData);

            sheetData.Append(CreateHeaderRow());
            uint rowIndex = 2;
            foreach (var tela in telas)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = new Row { RowIndex = rowIndex++ };
                row.Append(
                    TextCell(tela.Nombre),
                    NumberCell(tela.Gramatura),
                    tela.Ancho.HasValue ? NumberCell(tela.Ancho.Value) : TextCell(string.Empty));
                sheetData.Append(row);
            }

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Telas"
            });
            workbookPart.Workbook.Save();
        }

        stream.Position = 0;
        return Task.FromResult<Stream>(stream);
    }

    public Task<IReadOnlyList<Tela>> ImportAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var telas = new List<Tela>();

        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart
            ?? throw new InvalidDataException("El archivo no contiene un libro válido.");
        var workbook = workbookPart.Workbook
            ?? throw new InvalidDataException("El archivo no contiene un libro válido.");
        var firstSheet = workbook.Sheets?.Elements<Sheet>().FirstOrDefault()
            ?? throw new InvalidDataException("El archivo no contiene hojas.");
        var relationshipId = firstSheet.Id?.Value
            ?? throw new InvalidDataException("La hoja no tiene una referencia válida.");
        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(relationshipId);
        var worksheet = worksheetPart.Worksheet
            ?? throw new InvalidDataException("La hoja seleccionada no es válida.");
        var rows = worksheet.GetFirstChild<SheetData>()?.Elements<Row>().Skip(1)
            ?? Enumerable.Empty<Row>();

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cells = row.Elements<SpreadsheetCell>().ToList();
            var nombre = GetText(workbookPart, GetCell(cells, "A")).Trim();
            var gramaturaText = GetText(workbookPart, GetCell(cells, "B"));
            var anchoText = GetText(workbookPart, GetCell(cells, "C"));

            if (nombre.Length == 0 ||
                !double.TryParse(gramaturaText, NumberStyles.Float, CultureInfo.InvariantCulture, out var gramatura) ||
                !double.IsFinite(gramatura) ||
                gramatura <= 0)
            {
                continue;
            }

            double? ancho = null;
            if (double.TryParse(anchoText, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedWidth) &&
                double.IsFinite(parsedWidth) &&
                parsedWidth > 0)
            {
                ancho = parsedWidth;
            }

            telas.Add(new Tela { Nombre = nombre, Gramatura = gramatura, Ancho = ancho });
        }

        if (telas.Count == 0)
        {
            throw new InvalidDataException("No se encontraron filas válidas en el archivo.");
        }

        return Task.FromResult<IReadOnlyList<Tela>>(telas);
    }

    private static Stylesheet CreateStylesheet() =>
        new(
            new Fonts(
                new SpreadsheetFont(),
                new SpreadsheetFont(new Bold())),
            new Fills(new Fill(new PatternFill { PatternType = PatternValues.None })),
            new Borders(new SpreadsheetBorder()),
            new CellFormats(
                new CellFormat(),
                new CellFormat { FontId = 1, ApplyFont = true }));

    private static Row CreateHeaderRow()
    {
        var row = new Row { RowIndex = 1 };
        row.Append(HeaderCell("Nombre"), HeaderCell("Gramatura"), HeaderCell("Ancho (m)"));
        return row;
    }

    private static SpreadsheetCell HeaderCell(string value) => new()
    {
        DataType = CellValues.InlineString,
        InlineString = new InlineString(new Text(value)),
        StyleIndex = 1
    };

    private static SpreadsheetCell TextCell(string value) => new()
    {
        DataType = CellValues.InlineString,
        InlineString = new InlineString(new Text(value))
    };

    private static SpreadsheetCell NumberCell(double value) => new()
    {
        DataType = CellValues.Number,
        CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture))
    };

    private static SpreadsheetCell? GetCell(IEnumerable<SpreadsheetCell> cells, string column) =>
        cells.FirstOrDefault(cell =>
            cell.CellReference?.Value?.StartsWith(column, StringComparison.OrdinalIgnoreCase) == true);

    private static string GetText(WorkbookPart workbookPart, SpreadsheetCell? cell)
    {
        if (cell is null)
        {
            return string.Empty;
        }

        if (cell.DataType?.Value == CellValues.SharedString &&
            int.TryParse(cell.CellValue?.Text, out var sharedIndex))
        {
            return workbookPart.SharedStringTablePart?.SharedStringTable?
                .Elements<SharedStringItem>()
                .ElementAtOrDefault(sharedIndex)?
                .InnerText ?? string.Empty;
        }

        return cell.InlineString?.InnerText ?? cell.CellValue?.Text ?? string.Empty;
    }
}
