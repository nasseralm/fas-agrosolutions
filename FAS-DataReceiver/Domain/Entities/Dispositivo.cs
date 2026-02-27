namespace Agro.DataReceiver.Domain.Entities;

public sealed class Dispositivo
{
    public Guid Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string TalhaoId { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
