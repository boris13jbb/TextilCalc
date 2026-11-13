using System.Globalization;
using LiteDB;
using TextilCalc.App.Data;
using TextilCalc.App.Models;

namespace TextilCalc.App.Services;

public sealed class TelaRepository : ITelaRepository
{
    private const string CollectionName = "telas";
    private readonly string _databasePath =
        Path.Combine(FileSystem.AppDataDirectory, "gramatura_database.db");
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _initialized;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _writeLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            await Task.Run(() =>
            {
                using var database = OpenDatabase();
                var collection = database.GetCollection<Tela>(CollectionName);
                collection.EnsureIndex(tela => tela.Nombre);
                if (collection.Count() == 0)
                {
                    collection.InsertBulk(DefaultTelas.Create());
                }
                else
                {
                    NormalizeExistingNames(collection);
                }
            });
            _initialized = true;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<IReadOnlyList<Tela>> GetAllAsync(string? searchQuery = null)
    {
        await InitializeAsync();
        return await Task.Run<IReadOnlyList<Tela>>(() =>
        {
            using var database = OpenDatabase();
            var collection = database.GetCollection<Tela>(CollectionName);
            var telas = collection.Query().OrderBy(tela => tela.Nombre).ToList();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return telas;
            }

            var term = searchQuery.Trim();
            return telas
                .Where(tela => tela.Nombre.Contains(term, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        });
    }

    public async Task SaveAsync(Tela tela)
    {
        ArgumentNullException.ThrowIfNull(tela);
        Validate(tela);
        await InitializeAsync();
        await _writeLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var database = OpenDatabase();
                var collection = database.GetCollection<Tela>(CollectionName);
                if (tela.Id == 0)
                {
                    collection.Insert(tela);
                }
                else if (!collection.Update(tela))
                {
                    throw new InvalidOperationException("La tela que intenta actualizar ya no existe.");
                }
            });
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task DeleteAsync(Tela tela)
    {
        ArgumentNullException.ThrowIfNull(tela);
        await InitializeAsync();
        await _writeLock.WaitAsync();
        try
        {
            await Task.Run(() =>
            {
                using var database = OpenDatabase();
                database.GetCollection<Tela>(CollectionName).Delete(tela.Id);
            });
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task<(int Insertadas, int Actualizadas)> UpsertAsync(IReadOnlyList<Tela> telas)
    {
        ArgumentNullException.ThrowIfNull(telas);
        foreach (var tela in telas)
        {
            Validate(tela);
        }

        await InitializeAsync();
        await _writeLock.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                using var database = OpenDatabase();
                var collection = database.GetCollection<Tela>(CollectionName);
                var existing = collection.FindAll()
                    .GroupBy(tela => tela.Nombre, StringComparer.CurrentCultureIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.CurrentCultureIgnoreCase);
                var inserted = 0;
                var updated = 0;

                database.BeginTrans();
                try
                {
                    foreach (var imported in telas)
                    {
                        if (existing.TryGetValue(imported.Nombre, out var current))
                        {
                            current.Nombre = imported.Nombre;
                            current.Gramatura = imported.Gramatura;
                            current.Ancho = imported.Ancho ?? current.Ancho;
                            collection.Update(current);
                            updated++;
                        }
                        else
                        {
                            imported.Id = 0;
                            collection.Insert(imported);
                            existing[imported.Nombre] = imported;
                            inserted++;
                        }
                    }

                    database.Commit();
                    return (inserted, updated);
                }
                catch
                {
                    database.Rollback();
                    throw;
                }
            });
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private LiteDatabase OpenDatabase() =>
        new(new ConnectionString { Filename = _databasePath, Connection = ConnectionType.Direct });

    private static void Validate(Tela tela)
    {
        tela.Nombre = NormalizeNombre(tela.Nombre);
        if (tela.Nombre.Length == 0)
        {
            throw new ArgumentException("El nombre de la tela es obligatorio.", nameof(tela));
        }

        if (!double.IsFinite(tela.Gramatura) || tela.Gramatura <= 0)
        {
            throw new ArgumentException("La gramatura debe ser mayor que cero.", nameof(tela));
        }

        if (tela.Ancho is <= 0 || (tela.Ancho.HasValue && !double.IsFinite(tela.Ancho.Value)))
        {
            throw new ArgumentException("El ancho debe ser mayor que cero.", nameof(tela));
        }
    }

    /// <summary>
    /// Convierte nombres existentes a mayúsculas una sola vez al abrir la base.
    /// </summary>
    private static void NormalizeExistingNames(ILiteCollection<Tela> collection)
    {
        foreach (var tela in collection.FindAll())
        {
            var normalized = NormalizeNombre(tela.Nombre);
            if (string.Equals(tela.Nombre, normalized, StringComparison.Ordinal))
            {
                continue;
            }

            tela.Nombre = normalized;
            collection.Update(tela);
        }
    }

    private static string NormalizeNombre(string? nombre) =>
        (nombre ?? string.Empty).Trim().ToUpper(CultureInfo.CurrentCulture);
}
