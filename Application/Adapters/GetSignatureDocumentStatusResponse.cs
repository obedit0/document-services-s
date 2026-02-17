namespace Application.Adapters
{
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
        public string? S3Key { get; set; }  // Se llenará si Action es "USE_S3"
        public string? Url { get; set; }    // Se llenará si Action es "USE_CACHE"
    }
}