using Domain.Catalogs;
using FluentValidation;

namespace Application.Adapters.SignatureContracts;

public class GetOrderByProviderIdRequest
{
    public string? IdOrdenProveedor { get; set; }
}

public class GetOrderByProviderIdRequestValidator : AbstractValidator<GetOrderByProviderIdRequest>
{
    public GetOrderByProviderIdRequestValidator()
    {
        RuleFor(x => x.IdOrdenProveedor)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002");
    }
}
