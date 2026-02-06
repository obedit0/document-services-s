using Domain.Catalogs;
using FluentValidation;

namespace Application.Adapters;

public class CancelSignatureContractRequest
{
    public int? IdCanal { get; set; }
    public int? IdCanalTransaccion { get; set; }
}

public class CancelSignatureContractRequestValidator : AbstractValidator<CancelSignatureContractRequest>
{
    public CancelSignatureContractRequestValidator()
    {
        RuleFor(x => x.IdCanal)
            .NotNull().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .GreaterThan(0).WithMessage(MessageCatalog.GetErrorByCode(21013, "IdCanal")).WithErrorCode("21013");

        RuleFor(x => x.IdCanalTransaccion)
            .NotNull().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .GreaterThan(0).WithMessage(MessageCatalog.GetErrorByCode(21013, "IdCanalTransaccion")).WithErrorCode("21013");
    }
}