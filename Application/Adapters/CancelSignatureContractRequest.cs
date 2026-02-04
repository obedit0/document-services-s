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
            .GreaterThan(0).WithMessage("El IdCanal debe ser mayor a 0").WithErrorCode("21002");

        RuleFor(x => x.IdCanalTransaccion)
            .NotNull().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .GreaterThan(0).WithMessage("El IdCanalTransaccion debe ser mayor a 0").WithErrorCode("21002");
    }
}