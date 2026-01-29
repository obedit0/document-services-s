namespace KeynuaInfrastructure.Collections.Request;

internal sealed class KeynuaCreateContractRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Reference { get; set; }

    public string? TemplateId { get; set; }

    public int ExpirationInHours { get; set; }

    public List<KeynuaDocument> Documents { get; set; } = new();

    public List<KeynuaUser> Users { get; set; } = new();

    public KeynuaFlags? Flags { get; set; }
}

internal sealed class KeynuaDocument
{
    public string? Name { get; set; }

    public string? Base64 { get; set; }
}

internal sealed class KeynuaUser
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public List<string> Groups { get; set; } = new();
}

internal sealed class KeynuaFlags
{
    public KeynuaRemindersData? RemindersData { get; set; }

    public KeynuaPdfData? PDFData { get; set; }

    public List<string>? ChosenNotificationOptions { get; set; }
}

internal sealed class KeynuaRemindersData
{
    public int Frequency { get; set; }

    public int MaxAttempts { get; set; }
}

internal sealed class KeynuaPdfData
{
    public bool AddSignatureOnAllDocs { get; set; }
}
