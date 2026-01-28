namespace Domain.Entities.Internals;

public class MicroserviceErrorEntity
{
    public required string ErrorCode { get; set; }
    public int ChannelId { get; set; }
    public string? Endpoint { get; set; }
    public string? MessageIdentification { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? StackTrace { get; set; }
    public string? Message { get; set; }
    public string? Payload { get; set; }
    public string? Header { get; set; }
}
