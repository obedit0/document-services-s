using System.Text.Json.Serialization;
using Domain.Entities.Client;

namespace KeynuaInfrastructure.Collections.Request;

public class KeynuaSignatureRequest
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Reference { get; set; }
    public required List<KeynuaDocumentRequest> Documents { get; set; }
    public required string TemplateId { get; set; }
    public required string ExpirationDatetime { get; set; }
    public required List<KeynuaUserRequest> Users { get; set; }
    public required KeynuaFlagsRequest Flags { get; set; }
}


public class KeynuaDocumentRequest
{
    public required string Name { get; set; }
    public required string Base64 { get; set; }
    public required string RefId { get; set; }
}


public class KeynuaUserRequest
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public required string Phone { get; set; }
    public required List<string> Groups { get; set; }
    public required List<string> DocumentsRefs { get; set; }
    public required List<KeynuaPrefilledItemRequest> PrefilledItems { get; set; }
}

public class KeynuaPrefilledItemRequest
{
    public required string Target { get; set; }
    public required KeynuaValueRequest Value { get; set; }
}
public class KeynuaValueRequest
{
    public required string Text { get; set; }
}
public class KeynuaFlagsRequest
{
    public required KeynuaRemindersDataRequest RemindersData { get; set; }

    [JsonPropertyName("PDFData")]
    public required KeynuaPDFDataRequest PDFData { get; set; }
    public required List<string> ChosenNotificationOptions { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KeynuaCavaliDataRequest? CavaliData { get; set; }
}

public class KeynuaRemindersDataRequest
{
    public required int Frequency { get; set; }
    public required int MaxAttempts { get; set; }
}
public class KeynuaPDFDataRequest
{
    public required bool AddSignatureOnAllDocs { get; set; }
}

public class KeynuaCavaliDataRequest
{
    public NaturalClientEntity? Client { get; set; }
    public List<NaturalClientEntity>? Representatives { get; set; }
    public List<NaturalClientEntity>? Guarantees { get; set; }
    public int? UniqueCode { get; set; }
    public string? ClientName { get; set; }
    public string? Domicile { get; set; }
    public string? IssuedDate { get; set; }
    public string? Banking { get; set; }
    public string? Product { get; set; }
    public int? CreditNumber { get; set; }
}
