
using Domain.Enums;

namespace Domain.Entities.Client;

public sealed class IdentityDocumentEntity
{
    public DocumentType? Type { get; set; }
    public string? Number { get; set; }
}
