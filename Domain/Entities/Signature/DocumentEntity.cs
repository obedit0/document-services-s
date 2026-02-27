namespace Domain.Entities.SignatureContracts;

public class DocumentEntity
{
    public required string Name { get; set; }
    public List<string> OwnerClients { get; set; } = [];
    public required string S3KeyOriginal { get; set; }
    public string? S3KeyFirmado { get; set; }
    public DateTime? S3KeyFirmadoExpiresAt { get; set; }
    public string? ProviderKeyFirmado { get; set; }
    public DateTime? ProviderKeyFirmadoExpiresAt { get; set; }
    public DateTime? FechaFirma { get; set; }
}
