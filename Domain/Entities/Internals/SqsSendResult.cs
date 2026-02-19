namespace Domain.Entities.Internals;

public sealed class SqsSendResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
}
