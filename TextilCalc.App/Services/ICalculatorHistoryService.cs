using System.Collections.ObjectModel;
using TextilCalc.Core.Models;

namespace TextilCalc.App.Services;

public interface ICalculatorHistoryService
{
    ObservableCollection<CalculatorHistoryEntry> Entries { get; }

    void Add(string expression, string result);

    void Clear();

    void Load();
}
