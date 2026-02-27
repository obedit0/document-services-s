namespace AwsSqsInfrastructure;

public sealed class AwsSqsOptions
{
    public string? Region { get; set; }
    public string? QueueUrl { get; set; }
    public string? MessageGroupId { get; set; }
}
