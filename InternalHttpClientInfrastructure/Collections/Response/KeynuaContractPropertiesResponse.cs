namespace KeynuaInfrastructure.Collections.Response;

public sealed class KeynuaContractPropertiesResponse
{
    public string? Id { get; set; }

    public string? AccountId { get; set; }

    public string? SentBy { get; set; }

    public string? TemplateId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset? CanceledAt { get; set; }

    public string? Language { get; set; }

    public string? Timezone { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public string? Reference { get; set; }

    public string? ShortCode { get; set; }

    public int? ExpirationInHours { get; set; }

    public bool? Expired { get; set; }

    public int? ItemsCount { get; set; }

    public List<KeynuaGroup>? Groups { get; set; }

    public List<KeynuaUser>? Users { get; set; }

    public List<KeynuaDocument>? Documents { get; set; }

    public List<KeynuaItem>? Items { get; set; }

    public string? Status { get; set; }
}

public sealed class KeynuaGroup
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public bool? DigitalSignature { get; set; }

    public bool? Bulk { get; set; }

    public bool? IsApprover { get; set; }
}

public sealed class KeynuaUser
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public List<string>? Groups { get; set; }

    public KeynuaAttempt? Attempts { get; set; }

    public string? Token { get; set; }

    public string? State { get; set; }
}

public sealed class KeynuaAttempt
{
    public int? Max { get; set; }

    public int? Current { get; set; }
}

public sealed class KeynuaDocument
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? Ext { get; set; }

    public string? Sha { get; set; }

    public int? Size { get; set; }

    public string? Type { get; set; }

    public string? Url { get; set; }
}

public sealed class KeynuaItem
{
    public int? Id { get; set; }

    public int? Version { get; set; }

    public string? State { get; set; }

    public int? UserId { get; set; }

    public string? Reference { get; set; }

    public string? Title { get; set; }

    public string? Type { get; set; }

    public int? StageIndex { get; set; }

    public KeynuaItemValue? Value { get; set; }

    public bool? AllowsManualUpdate { get; set; }

    public bool? AllowsRetry { get; set; }

    public bool? HideOnWebApp { get; set; }
}

public sealed class KeynuaItemValue
{
    public string? Url { get; set; }

    public List<string>? IndividualDocsUrls { get; set; }

    public List<KeynuaIndividualDocDto>? IndividualDocs { get; set; }

    public bool? Success { get; set; }

    public string? ProcessId { get; set; }
}

public sealed class KeynuaIndividualDocDto
{
    public string? Bucket { get; set; }

    public string? Hash { get; set; }

    public string? Key { get; set; }
}
