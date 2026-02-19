namespace Application.Adapters.SignatureContracts;

public class CreateSignatureContractResponse
{
    public string? IdFirma { get; set; }
    public string? Referencia { get; set; }
    public string? Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
