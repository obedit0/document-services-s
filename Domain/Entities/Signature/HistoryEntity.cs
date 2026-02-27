using Domain.Enums;

namespace Domain.Entities.SignatureContracts;

public class HistoryEntity
{
    public DateTime EventDate { get; set; }
    public string? Source { get; set; }
    public SignatureStatus? PreviousStatus { get; set; }
    public SignatureStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public string? ActorId { get; set; }
    public string? ProviderEventId { get; set; }
}
