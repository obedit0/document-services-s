namespace Domain.Entities.SignatureContracts;

public class Cliente
{
    public required string IdCliente { get; set; }
    public required string TipoVinculo { get; set; }
    public required string NombreCompleto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}
