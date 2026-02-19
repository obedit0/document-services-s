using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entities.Internals;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AwsSqsInfrastructure;

public sealed class SqsMicroserviceCallTracePublisher : IMicroserviceCallTracePublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ISqsMessagePublisher _publisher;
    private readonly ILogger<SqsMicroserviceCallTracePublisher> _logger;

    public SqsMicroserviceCallTracePublisher(
        ISqsMessagePublisher publisher,
        ILogger<SqsMicroserviceCallTracePublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<SqsSendResult> SendAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        string messageBody;
        try
        {
            messageBody = JsonSerializer.Serialize(entity, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize MicroserviceCallTraceEntity TraceId={TraceId}", entity.TraceId);
            return new SqsSendResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }

        return await _publisher.SendAsync(messageBody, ct);
    }
}
