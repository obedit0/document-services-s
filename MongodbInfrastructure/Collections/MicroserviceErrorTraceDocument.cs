using Domain.Entities.Internals;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections;

[BsonIgnoreExtraElements]
public class MicroserviceErrorTraceDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("identity")]
    public string? Identity { get; set; }

    [BsonElement("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    [BsonElement("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [BsonElement("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    [BsonElement("request_url")]
    public string RequestUrl { get; set; } = string.Empty;

    [BsonElement("request_header")]
    public string? RequestHeader { get; set; }

    [BsonElement("request_payload")]
    public string? RequestPayload { get; set; }

    [BsonElement("error_message")]
    public string ErrorMessage { get; set; } = string.Empty;

    [BsonElement("error_stack_trace")]
    public string ErrorStackTrace { get; set; } = string.Empty;

    [BsonElement("datetime")]
    public DateTime Datetime { get; set; }

    [BsonElement("is_resolved")]
    public bool IsResolved { get; set; }

    public MicroserviceErrorTraceEntity ToDomain()
    {
        return new MicroserviceErrorTraceEntity
        {
            Identity = Identity,
            TraceId = TraceId,
            ChannelId = ChannelId,
            DeviceId = DeviceId,
            RequestUrl = RequestUrl,
            RequestHeader = RequestHeader,
            RequestPayload = RequestPayload,
            ErrorMessage = ErrorMessage,
            ErrorStackTrace = ErrorStackTrace,
            Datetime = Datetime,
            IsResolved = IsResolved
        };
    }

    public static MicroserviceErrorTraceDocument FromDomain(MicroserviceErrorTraceEntity entity)
    {
        return new MicroserviceErrorTraceDocument
        {
            Identity = entity.Identity,
            TraceId = entity.TraceId,
            ChannelId = entity.ChannelId,
            DeviceId = entity.DeviceId,
            RequestUrl = entity.RequestUrl,
            RequestHeader = entity.RequestHeader,
            RequestPayload = entity.RequestPayload,
            ErrorMessage = entity.ErrorMessage,
            ErrorStackTrace = entity.ErrorStackTrace,
            Datetime = entity.Datetime,
            IsResolved = entity.IsResolved
        };
    }
}
