namespace AwsSqsInfrastructure;

public sealed class AwsSqsOptions
{
    public string? Region { get; set; }
    public string? ServiceUrl { get; set; }
    public string? ProfileName { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? QueueUrl { get; set; }
    public string? MessageGroupId { get; set; }
    public string? MessageDeduplicationId { get; set; }
}
