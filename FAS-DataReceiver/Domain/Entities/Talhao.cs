namespace Agro.DataReceiver.Domain.Entities;

public sealed class Talhao
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string? GeoJson { get; set; }
    public DateTime UpdatedAt { get; set; }
}
