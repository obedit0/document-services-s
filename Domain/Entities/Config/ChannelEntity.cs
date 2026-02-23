namespace Domain.Entities.Config;

public class ChannelEntity
{
    public int IdCanal { get; set; }
    public bool Enabled { get; set; }
    public DocumentsUploadConfig? DocumentsUpload { get; set; }
    public UploadTimeWindowConfig? UploadTimeWindow { get; set; }
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DocumentsUploadConfig
{
    public long MaxTotalBytes { get; set; }
    public int MaxDocuments { get; set; }
}

public class UploadTimeWindowConfig
{
    public int FromMin { get; set; }
    public int ToMin { get; set; }
}
