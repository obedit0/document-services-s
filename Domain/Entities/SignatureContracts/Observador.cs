namespace Domain.Entities.SignatureContracts;

public class Observador
{
    public required string IdObservador { get; set; }
    public required string Email { get; set; }
    public string? Rol { get; set; }
}
