namespace TextilCalc.Core.Models;

public sealed record CalculatorHistoryEntry(
    string Expression,
    string Result,
    DateTimeOffset TimestampUtc);
