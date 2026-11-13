namespace TextilCalc.App.Models;

public sealed class Tela
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public double Gramatura { get; set; }

    public double? Ancho { get; set; }
}
