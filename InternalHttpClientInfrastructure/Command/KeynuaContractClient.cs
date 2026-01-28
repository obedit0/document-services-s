using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entities.SignatureContracts;
using Domain.Interfaces;
using InternalHttpClientInfrastructure.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InternalHttpClientInfrastructure.Queries;

public sealed class KeynuaContractClient : IKeynuaContractClient
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<KeynuaContractClient> _logger;
    private readonly KeynuaOptions _options;

    public KeynuaContractClient(IHttpClientFactory factory, ILogger<KeynuaContractClient> logger, IOptions<KeynuaOptions> options)
    {
        _factory = factory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<KeynuaContractResult> CreateContractAsync(KeynuaContractRequest request, CancellationToken ct = default)
    {
        var payload = BuildPayload(request);

        var builder = new HttpClientBuilder(_factory, _logger)
            .WithClient("ArifyClient")
            .WithBaseUrl(_options.BaseUrl)
            .WithEndpoint("contracts/v1")
            .WithHeader("x-api-key", _options.ApiKey)
            .WithHeader("authorization", _options.Authorization);

        HttpResponseResult<JsonElement> response;

        try
        {
            response = await builder.PutAsync<JsonElement>(payload, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("Keynua request failed: {Message}", ex.Message);
            return new KeynuaContractResult(false, 502, null, null, ex.Message);
        }

        if (!response.IsSuccess || response.Content.ValueKind == JsonValueKind.Undefined)
        {
            var raw = response.Content.ValueKind == JsonValueKind.Undefined
                ? null
                : response.Content.GetRawText();
            return new KeynuaContractResult(false, response.StatusCode, null, null, raw);
        }

        var providerId = TryGetString(response.Content, "id")
            ?? TryGetString(response.Content, "contractId")
            ?? TryGetString(response.Content, "contract_id")
            ?? TryGetString(response.Content, "idContract");

        var providerStatus = TryGetString(response.Content, "status")
            ?? TryGetString(response.Content, "state")
            ?? TryGetString(response.Content, "estado");

        return new KeynuaContractResult(true, response.StatusCode, providerId, providerStatus, response.Content.GetRawText());
    }

    private KeynuaCreateContractRequest BuildPayload(KeynuaContractRequest request)
    {
        var flags = new KeynuaFlags
        {
            PDFData = request.AddSignatureOnAllDocs.HasValue
                ? new KeynuaPdfData { AddSignatureOnAllDocs = request.AddSignatureOnAllDocs.Value }
                : null,
            ChosenNotificationOptions = request.ChosenNotificationOptions
        };

        return new KeynuaCreateContractRequest
        {
            Title = request.Title,
            Description = request.Description,
            Reference = request.Reference,
            TemplateId = _options.TemplateId,
            ExpirationInHours = _options.ExpirationInHours,
            Documents = request.Documents.Select(d => new KeynuaDocument
            {
                Name = d.Name,
                Base64 = d.Base64
            }).ToList(),
            Users = request.Users.Select(u => new KeynuaUser
            {
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Groups = u.Groups
            }).ToList(),
            Flags = flags
        };
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        if (!element.TryGetProperty(propertyName, out var value))
            return null;

        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
    }
}

internal sealed class KeynuaCreateContractRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("expirationInHours")]
    public int ExpirationInHours { get; set; }

    [JsonPropertyName("documents")]
    public List<KeynuaDocument> Documents { get; set; } = new();

    [JsonPropertyName("users")]
    public List<KeynuaUser> Users { get; set; } = new();

    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KeynuaFlags? Flags { get; set; }
}

internal sealed class KeynuaDocument
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("base64")]
    public string? Base64 { get; set; }
}

internal sealed class KeynuaUser
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phone { get; set; }

    [JsonPropertyName("groups")]
    public List<string> Groups { get; set; } = new();
}

internal sealed class KeynuaFlags
{
    [JsonPropertyName("remindersData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KeynuaRemindersData? RemindersData { get; set; }

    [JsonPropertyName("PDFData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KeynuaPdfData? PDFData { get; set; }

    [JsonPropertyName("chosenNotificationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ChosenNotificationOptions { get; set; }
}

internal sealed class KeynuaRemindersData
{
    [JsonPropertyName("frequency")]
    public int Frequency { get; set; }

    [JsonPropertyName("maxAttempts")]
    public int MaxAttempts { get; set; }
}

internal sealed class KeynuaPdfData
{
    [JsonPropertyName("addSignatureOnAllDocs")]
    public bool AddSignatureOnAllDocs { get; set; }
}
