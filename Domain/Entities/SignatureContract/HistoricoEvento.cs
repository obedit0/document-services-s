using Domain.Enums;

namespace Domain.Entities.SignatureContracts;

public class HistoricoEvento
{
    public DateTime FechaEvento { get; set; }
    public string? Fuente { get; set; }
    public EstadoFirma? EstadoAnterior { get; set; }
    public EstadoFirma EstadoNuevo { get; set; }
    public string? Motivo { get; set; }
    public string? ActorId { get; set; }
    public string? ProviderEventId { get; set; }
}
