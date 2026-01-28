using Domain.Catalogs;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Adapters;

public class CreateSignatureContractRequest
{
    public string? Referencia { get; set; }
    public string? Proveedor { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Canal { get; set; }
    public DateTimeOffset? HoraExpiracion { get; set; }
    public bool? FirmaEnTodosDocumentos { get; set; }
    public List<string>? IdTiposNotificacion { get; set; }
    public List<ClienteRequest>? Clientes { get; set; }
    public List<DocumentoRequest>? Documentos { get; set; }
    public List<ObservadorRequest>? Observadores { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ClienteRequest
{
    public string? IdCliente { get; set; }
    public string? TipoVinculo { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}

public class DocumentoRequest
{
    public string? IdDocumento { get; set; }
    public string? TipoDocumento { get; set; }
    public string? OwnerClienteId { get; set; }
    public string? S3KeyOriginal { get; set; }
    public string? HashSha256 { get; set; }
    public string? S3KeyFirmado { get; set; }
    public string? ProviderKeyFirmado { get; set; }
    public DateTimeOffset? FechaFirma { get; set; }
}

public class ObservadorRequest
{
    public string? IdObservador { get; set; }
    public string? Email { get; set; }
    public string? Rol { get; set; }
}

public class CreateSignatureContractRequestValidator : AbstractValidator<CreateSignatureContractRequest>
{
    public CreateSignatureContractRequestValidator()
    {
        RuleFor(x => x.Referencia)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Proveedor)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Clientes)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Documentos)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleForEach(x => x.Clientes)
            .SetValidator(new ClienteRequestValidator());

        RuleForEach(x => x.Documentos)
            .SetValidator(new DocumentoRequestValidator());

        RuleForEach(x => x.Observadores)
            .SetValidator(new ObservadorRequestValidator());

        RuleFor(x => x).Custom(ValidateOwnerClienteId);
    }

    private static void ValidateOwnerClienteId(CreateSignatureContractRequest request, ValidationContext<CreateSignatureContractRequest> context)
    {
        if (request.Documentos is null || request.Clientes is null)
            return;

        var clienteIds = request.Clientes
            .Where(x => !string.IsNullOrWhiteSpace(x.IdCliente))
            .Select(x => x.IdCliente!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var documento in request.Documentos)
        {
            if (documento is null)
                continue;

            var ownerId = documento.OwnerClienteId ?? string.Empty;
            if (ownerId.Length == 0 || !clienteIds.Contains(ownerId))
            {
                context.AddFailure(new ValidationFailure(nameof(CreateSignatureContractRequest.Documentos), MessageCatalog.GetErrorByCode(21007))
                {
                    ErrorCode = "21007"
                });
                return;
            }
        }
    }
}

public class ClienteRequestValidator : AbstractValidator<ClienteRequest>
{
    public ClienteRequestValidator()
    {
        RuleFor(x => x.IdCliente)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.TipoVinculo)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.NombreCompleto)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");
    }
}

public class DocumentoRequestValidator : AbstractValidator<DocumentoRequest>
{
    public DocumentoRequestValidator()
    {
        RuleFor(x => x.IdDocumento)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.TipoDocumento)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.OwnerClienteId)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.S3KeyOriginal)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .Must(NotBeUrl).WithMessage(MessageCatalog.GetErrorByCode(21007)).WithErrorCode("21007");
    }

    private static bool NotBeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return !value.Contains("://", StringComparison.OrdinalIgnoreCase) &&
               !value.StartsWith("http", StringComparison.OrdinalIgnoreCase);
    }
}

public class ObservadorRequestValidator : AbstractValidator<ObservadorRequest>
{
    public ObservadorRequestValidator()
    {
        RuleFor(x => x.IdObservador)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");
    }
}
