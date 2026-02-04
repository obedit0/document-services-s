namespace Domain.Entities.Client;

public sealed class NaturalClientEntity : ClientEntity
{
    public string? GivenName { get; set; }
    public string? FullName { get; set; }
    public string? PaternalLastName { get; set; }
    public string? MaternalLastName { get; set; }
}
