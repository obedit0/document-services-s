using Domain.Catalogs;
using Domain.Enums;
using FluentValidation;

namespace Application.Adapters.SignatureContracts;

public class GetSignatureStatusRequest
{
    public int? IdCanal { get; set; }
    public long? IdCanalTransaccion { get; set; }
}

public class GetSignatureStatusRequestValidator : AbstractValidator<GetSignatureStatusRequest>
{
    public GetSignatureStatusRequestValidator()
    {
        RuleFor(x => x.IdCanal)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .Must(IsValidChannel).WithMessage(MessageCatalog.GetErrorByCode(21007)).WithErrorCode("21007");

        RuleFor(x => x.IdCanalTransaccion)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .GreaterThan(0).WithMessage(MessageCatalog.GetErrorByCode(21003)).WithErrorCode("21003");
    }

    private static bool IsValidChannel(int? value)
    {
        if (!value.HasValue)
            return false;

        return Enum.IsDefined(typeof(Channel), value.Value);
    }
}
