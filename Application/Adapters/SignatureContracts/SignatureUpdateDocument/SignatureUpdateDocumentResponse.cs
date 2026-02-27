namespace Application.Adapters.SignatureContracts.UpdateDocuments;

public class SignatureUpdateDocumentResponse
{
    public string? IdFirma { get; set; }
    public string? Estado { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public int DocumentosActualizados { get; set; }
}
