namespace Application.Adapters.UpdateDocuments;

public class UpdateSignedDocumentsResponse
{
    public string? IdFirma { get; set; }
    public string? Estado { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public int DocumentosActualizados { get; set; }
}
