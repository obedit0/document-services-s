namespace InternalHttpClientInfrastructure;

public sealed class KeynuaOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Authorization { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public int ExpirationInHours { get; set; }
}
