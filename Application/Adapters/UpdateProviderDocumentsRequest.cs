using Domain.Catalogs;
using FluentValidation;

namespace Application.Adapters;

public class UpdateProviderDocumentsRequest
{
    public string? IdFirma { get; set; }
    public List<ProviderDocumentRequest>? Documents { get; set; }
}

public class ProviderDocumentRequest
{
    public string? Name { get; set; }
    public string? ProviderKeyFirmado { get; set; }
    public DateTime? UrlExpiresAt { get; set; }
}

public class UpdateProviderDocumentsRequestValidator : AbstractValidator<UpdateProviderDocumentsRequest>
{
    public UpdateProviderDocumentsRequestValidator()
    {
        RuleFor(x => x.IdFirma)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Documents)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleForEach(x => x.Documents)
            .SetValidator(new ProviderDocumentRequestValidator());
    }
}

public class ProviderDocumentRequestValidator : AbstractValidator<ProviderDocumentRequest>
{
    public ProviderDocumentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.ProviderKeyFirmado)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");
    }
}
