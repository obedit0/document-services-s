using Domain.Entities.Client;
using Domain.Enums;

namespace Domain.Entities.SignatureContracts;

public class SignatureEntity
{
    public string SignatureId { get; set; }
    public required string Reference { get; set; }
    public required int Keyword { get; set; }
    public string? ProviderIdentity { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required ChannelEnum Channel { get; set; }
    public required DateTime ExpirationTime { get; set; }
    public required List<string> NotificationTypeIds { get; set; }
    public required List<NaturalClientEntity> Clients { get; set; }
    public required List<DocumentEntity> Documents { get; set; }
    public SignatureStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<HistoryEntity>? History { get; set; }
    public required bool PromissoryNote { get; set; }
    public int? CreditNumber { get; set; }
}
