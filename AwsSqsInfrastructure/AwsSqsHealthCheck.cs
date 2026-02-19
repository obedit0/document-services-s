using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net;

namespace AwsSqsInfrastructure;

public sealed class AwsSqsHealthCheck : IHealthCheck
{
    private readonly IAmazonSQS _sqs;
    private readonly AwsSqsOptions _options;

    public AwsSqsHealthCheck(IAmazonSQS sqs, IOptions<AwsSqsOptions> options)
    {
        _sqs = sqs;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.QueueUrl))
            return HealthCheckResult.Unhealthy("SQS queue url is not configured");

        try
        {
            var response = await _sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
            {
                QueueUrl = _options.QueueUrl,
                AttributeNames = new List<string> { "QueueArn" }
            }, cancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
                return HealthCheckResult.Unhealthy($"SQS returned status {response.HttpStatusCode}");

            return HealthCheckResult.Healthy("SQS reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQS check failed", ex);
        }
    }
}
