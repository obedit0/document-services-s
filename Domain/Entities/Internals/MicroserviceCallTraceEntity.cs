namespace Domain.Entities.Internals;

public class MicroserviceCallTraceEntity
{
    public string? Identity { get; set; }
    public required string TraceId { get; set; }
    public required string ChannelId { get; set; }
    public required string DeviceId { get; set; }
    public string? Keyword { get; set; }  // usuario/dni/dispositivo
    public required string MicroserviceName { get; set; } = "SystemApi3.0"; // This Project
    public required string OperationName { get; set; } // Transfer.GetBalance.execute
    public required string RequestUrl { get; set; }
    public string? RequestHeader { get; set; }
    public string? RequestPayload { get; set; }
    public required DateTime RequestDatetime { get; set; } = DateTime.UtcNow;
    public required int ResponseStatusCode { get; set; }
    public string? ResponsePayload { get; set; }
    public DateTime ResponseDatetime { get; set; } = DateTime.UtcNow;
}
