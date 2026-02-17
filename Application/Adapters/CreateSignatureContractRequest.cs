using Domain.Catalogs;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Adapters;

public class CreateSignatureContractRequest
{
    public string? Referencia { get; set; }
    public string? Keyword { get; set; }
    public string? Titulo { get; set; }
    public string? Descripcion { get; set; }
    public string? Canal { get; set; }
    public DateTime? HoraExpiracion { get; set; }
    public required List<string> IdTiposNotificacion { get; set; }
    public bool FirmaEnTodoDocumentos { get; set; }
    public bool Pagare { get; set; }
    public List<DocumentoRequest>? Documentos { get; set; }
    public List<ClienteRequest>? Clientes { get; set; }
    public List<ObservadorRequest>? Observadores { get; set; }
}

public class ClienteRequest
{
    public string? IdCliente { get; set; }
    public string? TipoVinculo { get; set; }
    public string? NombreCompleto { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}

public class DocumentoRequest
{
    public string? Name { get; set; }
    public List<string>? OwnerClientId { get; set; }
    public string? S3KeyOriginal { get; set; }
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
            .Where(x => !string.IsNullOrWhiteSpace(x.NumeroDocumento))
            .Select(x => x.NumeroDocumento!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var documento in request.Documentos)
        {
            if (documento?.OwnerClientId is null || documento.OwnerClientId.Count == 0)
                continue;

            foreach (var ownerId in documento.OwnerClientId)
            {
                if (!clienteIds.Contains(ownerId))
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
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.OwnerClientId)
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
