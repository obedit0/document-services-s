namespace Domain.Entities.Client;

public sealed class AddressEntity
{
    public int? Identity {  get; set; }
    public string? Name { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Reference { get; set; }
    public string? PostalCode { get; set; }
}

