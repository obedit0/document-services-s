namespace Domain.Entities.Internals;

public class MicroserviceErrorTraceEntity
{
    public string? Identity { get; set; }
    public required string TraceId { get; set; }
    public required string ChannelId { get; set; }
    public required string DeviceId { get; set; }
    public required string RequestUrl { get; set; }
    public string? RequestHeader { get; set; }
    public string? RequestPayload { get; set; }
    public required string ErrorMessage { get; set; }
    public required string ErrorStackTrace { get; set; }
    public DateTime Datetime { get; set; } = DateTime.UtcNow;
    public required bool IsResolved { get; set; } = false;
}
