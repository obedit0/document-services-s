using Domain.Catalogs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
namespace Application.Adapters;

public class SignatureHeaderRequest
{
    [FromHeader(Name = "MessageIdentification")]
    public string? MessageIdentification { get; set; }
}

public class SignatureHeaderRequestValidator : AbstractValidator<SignatureHeaderRequest>
{
    public SignatureHeaderRequestValidator()
    {
        RuleFor(x => x.MessageIdentification)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .MinimumLength(5).WithMessage(MessageCatalog.GetErrorByCode(21004)).WithErrorCode("21004")
            .MaximumLength(42).WithMessage(MessageCatalog.GetErrorByCode(21005)).WithErrorCode("21005");
    }
}
