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
            Version = entity.Version,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

[BsonIgnoreExtraElements]
public class DocumentsUploadDocument
{
    [BsonElement("allowedWindow")]
    public AllowedWindowDocument? AllowedWindow { get; set; }

    [BsonElement("maxTotalBytes")]
    public long MaxTotalBytes { get; set; }

    [BsonElement("maxDocuments")]
    public int MaxDocuments { get; set; }

    public DocumentsUploadConfig ToDomain()
    {
        return new DocumentsUploadConfig
        {
            AllowedWindow = AllowedWindow?.ToDomain(),
            MaxTotalBytes = MaxTotalBytes,
            MaxDocuments = MaxDocuments
        };
    }

    public static DocumentsUploadDocument FromDomain(DocumentsUploadConfig config)
    {
        return new DocumentsUploadDocument
        {
            AllowedWindow = config.AllowedWindow != null 
                ? AllowedWindowDocument.FromDomain(config.AllowedWindow) 
                : null,
            MaxTotalBytes = config.MaxTotalBytes,
            MaxDocuments = config.MaxDocuments
        };
    }
}

[BsonIgnoreExtraElements]
public class AllowedWindowDocument
{
    [BsonElement("fromMin")]
    public int FromMin { get; set; }

    [BsonElement("toMin")]
    public int ToMin { get; set; }

    public AllowedWindowConfig ToDomain()
    {
        return new AllowedWindowConfig
        {
            FromMin = FromMin,
            ToMin = ToMin
        };
    }

    public static AllowedWindowDocument FromDomain(AllowedWindowConfig config)
    {
        return new AllowedWindowDocument
        {
            FromMin = config.FromMin,
            ToMin = config.ToMin
        };
    }
}
