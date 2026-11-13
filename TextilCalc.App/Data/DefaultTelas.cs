using TextilCalc.App.Models;

namespace TextilCalc.App.Data;

internal static class DefaultTelas
{
    public static IReadOnlyList<Tela> Create() =>
    [
        New("Albina", 2.53), New("Alfa", 1.82), New("Alfa Bw", 1.81), New("Alpes", 1.93),
        New("Amalfi Plus", 0), New("Ancor", 2.35), New("Aston", 2.06), New("Aston Bw", 2.07),
        New("Bahía", 2.08), New("Bahía Black", 2.17), New("Baltimore", 2.1), New("Baltimore Bw", 2.04),
        New("Baltra", 1.79), New("Balwin power EQ", 1.98), New("Brave", 1.47), New("Bref", 1.44),
        New("Broker", 1.78), New("Broker Baby blue pus", 1.65), New("Broker Black", 1.76),
        New("Broker nat", 1.75), New("Broker plus", 1.62), New("Broker Blak Plus", 1.65),
        New("Broker PT", 1.92), New("Cayman", 2.42), New("Charlette Black Bw", 1.89),
        New("Charlette Nigt", 1.94), New("Charlotte Power", 1.92), New("Charlotte", 1.85),
        New("Charlotte BW", 1.91), New("Charlotte Vitage", 1.92), New("Copper", 1.23),
        New("Cosmos", 1.48), New("Cosmos Bw", 2.04), New("Dover", 1.48), New("Draco", 1.53),
        New("Fermo", 2.42), New("Fix", 2.37), New("Florencia", 2.07), New("Fontana", 2.62),
        New("Fontana Bw", 2.55), New("Gaviota Bw", 2.37), New("Gaviota Black Bw", 2.42),
        New("Gaviota Plus", 2.36), New("Genova Hat", 2.3), New("Genova Black Bw", 1.25),
        New("Genova", 2.3), New("Georgia", 1.85), New("Ginette", 0), New("Ginette Plus", 2.44),
        New("Gregor 12", 1.47), New("Gregor 12 Plus", 1.39), New("Gregor 14", 1.24),
        New("Gregor Black", 1.48), New("Gregor PT", 1.6), New("Gregor 14", 1.21),
        New("Habana", 2.24), New("Habana Bw", 2.34), New("Haiti  EQ", 2.53),
        New("Haiti plus EQ", 2.49), New("Hanks plus EQ", 1.95), New("Himalaya", 1.54),
        New("Houston", 1.9), New("Iliniza", 1.98), New("Indiana", 1.53), New("Ivana", 2.2),
        New("Jasmine", 2.55), New("Kiana", 2.18), New("Lewis", 1.85), New("Loto", 2.45),
        New("Loto BW", 2.36), New("Luisiana", 1.98), New("Madeira", 1.8), New("Martina", 1.94),
        New("Master Power", 2.51), New("Max power ", 2.57), New("Melinda", 2.2), New("Miki", 0),
        New("Mistral", 2.3), New("Modric", 1.49), New("Moretti", 1.3), New("Moretti Black", 1.61),
        New("Moretti Blu Black", 1.61), New("Moretti Plus", 1.5), New("Napa", 2),
        New("Natural Darck", 2.23), New("Natural Flex", 2.23), New("Nebraska", 2.1),
        New("Nevada", 2.46), New("Nevada plus EQ", 2.38), New("Paula", 2.13),
        New("Paula Black", 2.2), New("Paula Darck", 2.2), New("Paula Vitage", 2.15),
        New("Paulina Black", 2.05), New("Pekin", 2.08), New("Romina", 2.52), New("Rosario", 1.95),
        New("Rover", 1.72), New("Royce", 1.81), New("Sabana Power ", 1.98), New("Salamanca", 1.96),
        New("Sani", 1.9), New("Spencer EQ", 2.48), New("Spencer Plus EQ", 2.56),
        New("Steven plus EQ", 2.25), New("Trinity", 1.8), New("Vic Max EQ", 2.75),
        New("Vic Max Power EQ", 2.8), New("Vodka", 2.33), New("Volga", 1.52),
        New("Zafiro Black", 2.95), New("Comaneci EQ", 2.6), New("HIMALAYA", 1.63), New("ELIO", 1.61)
    ];

    private static Tela New(string nombre, double gramatura) =>
        new() { Nombre = nombre.Trim().ToUpper(System.Globalization.CultureInfo.CurrentCulture), Gramatura = gramatura };
}
