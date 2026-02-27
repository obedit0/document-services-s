using Amazon.SQS;
using Amazon.SQS.Model;
using Domain.Entities.Internals;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace AwsSqsInfrastructure;

public sealed class SqsMessagePublisher : ISqsMessagePublisher
{
    private readonly IAmazonSQS _sqs;
    private readonly AwsSqsOptions _options;
    private readonly ILogger<SqsMessagePublisher> _logger;

    public SqsMessagePublisher(IAmazonSQS sqs, IOptions<AwsSqsOptions> options, ILogger<SqsMessagePublisher> logger)
    {
        _sqs = sqs;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SqsSendResult> SendAsync(string messageBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(messageBody))
        {
            return new SqsSendResult
            {
                IsSuccess = false,
                ErrorMessage = "SQS message body is empty"
            };
        }

        if (string.IsNullOrWhiteSpace(_options.QueueUrl))
        {
            return new SqsSendResult
            {
                IsSuccess = false,
                ErrorMessage = "SQS queue url is not configured"
            };
        }

        try
        {
            var isFifoQueue = _options.QueueUrl.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase);

            var request = new SendMessageRequest
            {
                QueueUrl = _options.QueueUrl,
                MessageBody = messageBody
            };

            if (isFifoQueue)
            {
                if (string.IsNullOrWhiteSpace(_options.MessageGroupId))
                {
                    return new SqsSendResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "MessageGroupId is required for FIFO queues"
                    };
                }

                request.MessageGroupId = _options.MessageGroupId;
                request.MessageDeduplicationId = Guid.NewGuid().ToString();
            }

            var response = await _sqs.SendMessageAsync(request, ct);
            var isOk = response.HttpStatusCode == HttpStatusCode.OK;

            return new SqsSendResult
            {
                IsSuccess = isOk,
                MessageId = response.MessageId,
                ErrorMessage = isOk ? null : $"SQS returned status {response.HttpStatusCode}"
            };
        }
        catch (AmazonSQSException ex)
        {
            _logger.LogError(
                ex,
                "SQS send failed (StatusCode={StatusCode}, ErrorCode={ErrorCode})",
                ex.StatusCode,
                ex.ErrorCode);
            return new SqsSendResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQS send failed");
            return new SqsSendResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
