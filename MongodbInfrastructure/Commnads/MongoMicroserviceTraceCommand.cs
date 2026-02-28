using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entities.Internals;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongodbInfrastructure.Collections;

namespace MongodbInfrastructure.Commnads;

public sealed class MongoMicroserviceTraceCommand : IMicroserviceTraceRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IMongoCollection<MicroserviceErrorTraceDocument> _errorCollection;
    private readonly ISqsMessagePublisher _sqsPublisher;
    private readonly ILogger<MongoMicroserviceTraceCommand> _logger;

    public MongoMicroserviceTraceCommand(
        IMongoDatabase database,
        ISqsMessagePublisher sqsPublisher,
        ILogger<MongoMicroserviceTraceCommand> logger)
    {
        _errorCollection = database.GetCollection<MicroserviceErrorTraceDocument>("MicroserviceErrorTrace");
        _sqsPublisher = sqsPublisher;
        _logger = logger;
        EnsureIndexes();
    }

    public async Task InsertErrorAsync(MicroserviceErrorTraceEntity entity, CancellationToken ct = default)
    {
        var document = MicroserviceErrorTraceDocument.FromDomain(entity);
        await _errorCollection.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task<SqsSendResult> PublishCallAsync(MicroserviceCallTraceEntity entity, CancellationToken ct = default)
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

        return await _sqsPublisher.SendAsync(messageBody, ct);
    }

    private void EnsureIndexes()
    {
        var indexes = new List<CreateIndexModel<MicroserviceErrorTraceDocument>>
        {
            new(
                Builders<MicroserviceErrorTraceDocument>.IndexKeys.Ascending(x => x.TraceId),
                new CreateIndexOptions { Name = "ix_error_trace_trace_id" }
            ),
            new(
                Builders<MicroserviceErrorTraceDocument>.IndexKeys.Ascending(x => x.ChannelId),
                new CreateIndexOptions { Name = "ix_error_trace_channel_id" }
            ),
            new(
                Builders<MicroserviceErrorTraceDocument>.IndexKeys.Descending(x => x.Datetime),
                new CreateIndexOptions { Name = "ix_error_trace_datetime" }
            )
        };

        _errorCollection.Indexes.CreateMany(indexes);
    }
}
