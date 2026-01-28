namespace Application.Adapters;

public class CreateSignatureContractResponse
{
    public string? IdFirma { get; set; }
    public string? Referencia { get; set; }
    public string? Estado { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }
}
