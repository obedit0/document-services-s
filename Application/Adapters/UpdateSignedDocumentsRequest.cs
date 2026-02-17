using Domain.Catalogs;
using FluentValidation;

namespace Application.Adapters;

public class UpdateSignedDocumentsRequest
{
    public string? IdFirma { get; set; }
    public List<SignedDocumentRequest>? Documents { get; set; }
}

public class SignedDocumentRequest
{
    public string? Name { get; set; }
    public string? S3KeyFirmado { get; set; }
    public DateTime? UrlExpiresAt { get; set; }
}

public class UpdateSignedDocumentsRequestValidator : AbstractValidator<UpdateSignedDocumentsRequest>
{
    public UpdateSignedDocumentsRequestValidator()
    {
        RuleFor(x => x.IdFirma)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.Documents)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleForEach(x => x.Documents)
            .SetValidator(new SignedDocumentRequestValidator());
    }
}

public class SignedDocumentRequestValidator : AbstractValidator<SignedDocumentRequest>
{
    public SignedDocumentRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");

        RuleFor(x => x.S3KeyFirmado)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");
    }
}
