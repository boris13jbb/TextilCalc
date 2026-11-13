using TextilCalc.App.Models;

namespace TextilCalc.App.Services;

public interface IExcelService
{
    Task<Stream> ExportAsync(IReadOnlyList<Tela> telas, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tela>> ImportAsync(Stream stream, CancellationToken cancellationToken = default);
}
