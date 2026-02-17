using FluentValidation;

namespace Application.Adapters;

public class GetSignatureDocumentStatusValidator : AbstractValidator<GetSignatureDocumentStatusRequest>
{
    public GetSignatureDocumentStatusValidator()
    {
        RuleFor(x => x.IdCanal)
            .NotNull().WithMessage("IdCanal es requerido")
            .GreaterThan(0).WithMessage("IdCanal debe ser mayor a 0");

        RuleFor(x => x.IdCanalTransaccion)
            .NotNull().WithMessage("IdCanalTransaccion es requerido")
            .GreaterThan(0).WithMessage("IdCanalTransaccion debe ser mayor a 0");
    }
}