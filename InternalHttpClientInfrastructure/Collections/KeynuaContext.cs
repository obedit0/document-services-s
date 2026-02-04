namespace InternalHttpClientInfrastructure.Collections;

public sealed class KeynuaContext
{
    public string BaseUrl { get; set; } = "https://api.stg.keynua.com";
    public string ApiKey { get; set; } = "NBJEbBJ3AG6hrPdKEXuOe5GbCNpjOM9A3uCZE4qd";
    public string Authorization { get; set; } = "ZmMwZWExNTYtNGUxNi00OTNiLWFiNjktZTFkNDkxNTg4MjliOmU5YzJkNDQwMDlmMjQ1YWJiOTE5N2ZmMTA0ZTk0YTlhOmNiMzkzZDAwM2JkZWZiM2M2Y2FiNGNiMDY2OTNiMzZjMzMzMzZiMzIzNTRiMDNiMTQ2MGI5ZWQzYzE4YmQ3Nzg";
    public string TemplateId { get; set; } = string.Empty;
    public int ExpirationInHours { get; set; }
}
