namespace Domain.Entities.SignatureContracts;

public class Documento
{
    public required string IdDocumento { get; set; }
    public required string TipoDocumento { get; set; }
    public required string NombreDocumento { get; set; }
    public required string OwnerClient { get; set; }
    public required string S3KeyOriginal { get; set; }
    public string? HashSha256 { get; set; }
    public string? S3KeyFirmado { get; set; }
    public string? ProviderKeyFirmado { get; set; }
    public DateTimeOffset? FechaFirma { get; set; }
}
