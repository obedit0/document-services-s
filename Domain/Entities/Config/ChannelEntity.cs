namespace Domain.Entities.Config;

public class ChannelEntity
{
    public int IdCanal { get; set; }
    public bool Enabled { get; set; }
    public DocumentsUploadConfig? DocumentsUpload { get; set; }
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DocumentsUploadConfig
{
    public AllowedWindowConfig? AllowedWindow { get; set; }
    public long MaxTotalBytes { get; set; }
    public int MaxDocuments { get; set; }
}

public class AllowedWindowConfig
{
    public int FromMin { get; set; }
    public int ToMin { get; set; }
}
