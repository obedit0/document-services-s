namespace Application.Adapters.SignatureContracts;

public class GetSignatureDocumentStatusResponse
{
    public string IdFirma { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string IdOrdenProveedor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public List<DocumentStatusDto> Documentos { get; set; } = new();
}

public class DocumentStatusDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? S3Key { get; set; }
    public string? Url { get; set; }
}
