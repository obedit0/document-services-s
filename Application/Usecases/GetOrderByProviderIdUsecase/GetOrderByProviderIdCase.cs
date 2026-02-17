using Application.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Interfaces;

namespace Application.Usecases.GetOrderByProviderIdUsecase;

public class GetOrderByProviderIdCase : IGetOrderByProviderIdPort
{
    private readonly IOrdenFirmaRepository _repository;

    public GetOrderByProviderIdCase(IOrdenFirmaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EasyResult<GetOrderByProviderIdResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        GetOrderByProviderIdRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var ordenFirma = await _repository.GetByProviderIdAsync(request.IdOrdenProveedor!, ct);
        if (ordenFirma is null)
        {
            return EasyResult<GetOrderByProviderIdResponse>.Failure(404,
            [
                new() { Code = "21010", Message = "Orden de firma no encontrada", Field = "IdOrdenProveedor" }
            ]);
        }

        return EasyResult<GetOrderByProviderIdResponse>.Success(new GetOrderByProviderIdResponse
        {
            IdFirma = ordenFirma.IdFirma,
            IdOrdenProveedor = ordenFirma.IdOrdenProveedor,
            Keyword = ordenFirma.Keyword,
            Estado = ordenFirma.Estado.ToString()
        });
    }

    private async Task<EasyResult<GetOrderByProviderIdResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        GetOrderByProviderIdRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new GetOrderByProviderIdRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<GetOrderByProviderIdResponse>.Failure(422, errors)
            : null;
    }
}
