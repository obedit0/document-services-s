using Application.Adapters.Common;
using Application.Adapters.Sqs;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Catalogs;
using Domain.Interfaces;

namespace Application.Usecases.SqsUsecase;

public class SqsTestCase : ISqsTestPort
{
    private readonly ISqsMessagePublisher _publisher;

    public SqsTestCase(ISqsMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task<EasyResult<SqsSendTestResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SqsSendTestRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var sendResult = await _publisher.SendAsync(request.Message!, ct);
        if (!sendResult.IsSuccess)
        {
            return EasyResult<SqsSendTestResponse>.Failure(502,
            [
                new ValidationResultAdapter(
                    "21098",
                    sendResult.ErrorMessage ?? MessageCatalog.GetErrorByCode(21098),
                    "Message")
            ]);
        }

        return EasyResult<SqsSendTestResponse>.Success(new SqsSendTestResponse
        {
            MessageId = sendResult.MessageId,
            SentAt = DateTime.UtcNow.AddHours(-5)
        });
    }

    private async Task<EasyResult<SqsSendTestResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        SqsSendTestRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new SqsSendTestRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<SqsSendTestResponse>.Failure(422, errors)
            : null;
    }
}
