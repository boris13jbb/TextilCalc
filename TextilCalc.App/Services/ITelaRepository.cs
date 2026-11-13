using TextilCalc.App.Models;

namespace TextilCalc.App.Services;

public interface ITelaRepository
{
    Task InitializeAsync();
    Task<IReadOnlyList<Tela>> GetAllAsync(string? searchQuery = null);
    Task SaveAsync(Tela tela);
    Task DeleteAsync(Tela tela);
    Task<(int Insertadas, int Actualizadas)> UpsertAsync(IReadOnlyList<Tela> telas);
}
