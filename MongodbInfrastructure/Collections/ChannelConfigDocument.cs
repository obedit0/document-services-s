using Domain.Entities.Config;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongodbInfrastructure.Collections;

[BsonIgnoreExtraElements]
public class ChannelConfigDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("idCanal")]
    public int IdCanal { get; set; }

    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    [BsonElement("documentsUpload")]
    public DocumentsUploadDocument? DocumentsUpload { get; set; }

    [BsonElement("uploadTimeWindow")]
    public UploadTimeWindowDocument? UploadTimeWindow { get; set; }

    [BsonElement("version")]
    public int Version { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public ChannelEntity ToDomain()
    {
        return new ChannelEntity
        {
            IdCanal = IdCanal,
            Enabled = Enabled,
            DocumentsUpload = DocumentsUpload?.ToDomain(),
            UploadTimeWindow = UploadTimeWindow?.ToDomain() ?? DocumentsUpload?.LegacyAllowedWindow?.ToDomain(),
            Version = Version,
            UpdatedAt = UpdatedAt
        };
    }

    public static ChannelConfigDocument FromDomain(ChannelEntity entity)
    {
        return new ChannelConfigDocument
        {
            IdCanal = entity.IdCanal,
            Enabled = entity.Enabled,
            DocumentsUpload = entity.DocumentsUpload != null 
                ? DocumentsUploadDocument.FromDomain(entity.DocumentsUpload) 
                : null,
            UploadTimeWindow = entity.UploadTimeWindow != null
                ? UploadTimeWindowDocument.FromDomain(entity.UploadTimeWindow)
                : null,
            Version = entity.Version,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

[BsonIgnoreExtraElements]
public class DocumentsUploadDocument
{
    [BsonElement("allowedWindow")]
    public UploadTimeWindowDocument? LegacyAllowedWindow { get; set; }

    [BsonElement("maxTotalBytes")]
    public long MaxTotalBytes { get; set; }

    [BsonElement("maxDocuments")]
    public int MaxDocuments { get; set; }

    public DocumentsUploadConfig ToDomain()
    {
        return new DocumentsUploadConfig
        {
            MaxTotalBytes = MaxTotalBytes,
            MaxDocuments = MaxDocuments
        };
    }

    public static DocumentsUploadDocument FromDomain(DocumentsUploadConfig config)
    {
        return new DocumentsUploadDocument
        {
            MaxTotalBytes = config.MaxTotalBytes,
            MaxDocuments = config.MaxDocuments
        };
    }
}

[BsonIgnoreExtraElements]
public class UploadTimeWindowDocument
{
    [BsonElement("fromMin")]
    public int FromMin { get; set; }

    [BsonElement("toMin")]
    public int ToMin { get; set; }

    public UploadTimeWindowConfig ToDomain()
    {
        return new UploadTimeWindowConfig
        {
            FromMin = FromMin,
            ToMin = ToMin
        };
    }

    public static UploadTimeWindowDocument FromDomain(UploadTimeWindowConfig config)
    {
        return new UploadTimeWindowDocument
        {
            FromMin = config.FromMin,
            ToMin = config.ToMin
        };
    }
}
