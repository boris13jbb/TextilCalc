using System.Collections.ObjectModel;
using System.Text.Json;
using TextilCalc.Core.Models;

namespace TextilCalc.App.Services;

/// <summary>
/// Persistencia del historial de operaciones válidas de la calculadora (máximo 50).
/// </summary>
public sealed class CalculatorHistoryService : ICalculatorHistoryService
{
    private const string StorageKey = "calculadora_history_v1";
    private const int MaxEntries = 50;

    public ObservableCollection<CalculatorHistoryEntry> Entries { get; } = [];

    public CalculatorHistoryService()
    {
        Load();
    }

    public void Add(string expression, string result)
    {
        if (string.IsNullOrWhiteSpace(expression) || string.IsNullOrWhiteSpace(result))
        {
            return;
        }

        var entry = new CalculatorHistoryEntry(expression.Trim(), result.Trim(), DateTimeOffset.UtcNow);
        Entries.Insert(0, entry);

        while (Entries.Count > MaxEntries)
        {
            Entries.RemoveAt(Entries.Count - 1);
        }

        Persist();
    }

    public void Clear()
    {
        Entries.Clear();
        Preferences.Default.Remove(StorageKey);
    }

    public void Load()
    {
        Entries.Clear();
        var json = Preferences.Default.Get(StorageKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            var loaded = JsonSerializer.Deserialize<List<CalculatorHistoryEntry>>(json);
            if (loaded is null)
            {
                return;
            }

            foreach (var entry in loaded.Take(MaxEntries))
            {
                Entries.Add(entry);
            }
        }
        catch (JsonException)
        {
            Preferences.Default.Remove(StorageKey);
        }
    }

    private void Persist()
    {
        var payload = Entries.Take(MaxEntries).ToList();
        Preferences.Default.Set(StorageKey, JsonSerializer.Serialize(payload));
    }
}
