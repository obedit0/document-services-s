using Domain.Catalogs;
using FluentValidation;

namespace Application.Adapters.SignatureContracts;

public class SignatureCancellationRequest
{
    public int? Keyword { get; set; }
}

public class CancelSignatureContractRequestValidator : AbstractValidator<SignatureCancellationRequest>
{
    public CancelSignatureContractRequestValidator()
    {
        RuleFor(x => x.Keyword)
            .NotNull().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .GreaterThan(0).WithMessage(MessageCatalog.GetErrorByCode(21013, "Keyword")).WithErrorCode("21013");
    }
}
