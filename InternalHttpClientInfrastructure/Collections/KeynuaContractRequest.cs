namespace Domain.Entities.SignatureContracts;

public class KeynuaContractRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string Reference { get; set; }
    public required List<KeynuaContractDocument> Documents { get; set; }
    public required List<KeynuaContractUser> Users { get; set; }
    public bool? AddSignatureOnAllDocs { get; set; }
    public List<string>? ChosenNotificationOptions { get; set; }
}

public class KeynuaContractDocument
{
    public required string Name { get; set; }
    public required string Base64 { get; set; }
}

public class KeynuaContractUser
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public required List<string> Groups { get; set; }
}
