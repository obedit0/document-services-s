namespace Domain.Entities.SignatureContracts;

public sealed record KeynuaContractResult(
    bool IsSuccess,
    int StatusCode,
    string? ProviderId,
    string? ProviderStatus,
    string? RawResponse
);
