using Domain.Catalogs;
using FluentValidation;

namespace Application.Adapters.Sqs;

public class SqsSendTestRequest
{
    public string? Message { get; set; }
}

public class SqsSendTestRequestValidator : AbstractValidator<SqsSendTestRequest>
{
    public SqsSendTestRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(MessageCatalog.GetErrorByCode(21002)).WithErrorCode("21002")
            .MaximumLength(1024).WithMessage(MessageCatalog.GetErrorByCode(21005)).WithErrorCode("21005");
    }
}

public class SqsSendTestResponse
{
    public string? MessageId { get; set; }
    public DateTime SentAt { get; set; }
}
