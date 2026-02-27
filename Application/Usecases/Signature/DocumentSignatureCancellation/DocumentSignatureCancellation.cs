using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Adapters.SignatureContracts.DocumentSignatureCompletion;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Application.Usecases.Signature;
using Domain.Catalogs;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.Signature.SignatureContractCancellation;

public class DocumentSignatureCancellation : ICancelSignatureContractPort
{
    private readonly ISignatureQuery _signatureQuery;
    private readonly ISignatureCommand _signatureCommand;
    private readonly ISignatureContractCommand _contractCommand;

    public DocumentSignatureCancellation(
        ISignatureQuery signatureQuery,
        ISignatureCommand signatureCommand,
        ISignatureContractCommand contractCommand)
    {
        _signatureQuery = signatureQuery;
        _signatureCommand = signatureCommand;
        _contractCommand = contractCommand;
    }

    public async Task<EasyResult<SignatureCancellationResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SignatureCancellationRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification);
        if (canal is null)
            return EasyResult<SignatureCancellationResponse>.Failure(422, [new ValidationResultAdapter("21008", "Canal no definido", "Canal")]);

        var orden = await _signatureQuery.GetByKeywordAndChannelAsync(request.Keyword!.Value, (int)canal, ct);
        if (orden == null)
            return EasyResult<SignatureCancellationResponse>.Failure(409, [new ValidationResultAdapter("21008", "Orden de firma No existe", "Keyword")]);

        var newStatus = orden.Status == SignatureStatus.COMPLETADO ? SignatureStatus.ANULADO : SignatureStatus.CANCELADO;

        if (orden.Status != SignatureStatus.COMPLETADO)
            await _contractCommand.CancelAsync(orden.ProviderIdentity!, ct);

        var history = new HistoryEntity
        {
            EventDate = DateTime.UtcNow.AddHours(-5),
            Source = "API",
            PreviousStatus = orden.Status,
            NewStatus = newStatus,
            Reason = newStatus == SignatureStatus.CANCELADO ? "Cancelación manual de orden de firma" : "Anulación de orden de firma completada",
            ActorId = header.MessageIdentification
        };

        await _signatureCommand.CancellationAsync(orden.SignatureId, newStatus, history, ct);

        return EasyResult<SignatureCancellationResponse>.Success(new SignatureCancellationResponse
        {
            Message = newStatus == SignatureStatus.CANCELADO ? "Orden de firma cancelada exitosamente." : "Orden de firma anulada exitosamente.",
            Estado = newStatus.ToString()
        });

    }

    private async Task<EasyResult<SignatureCancellationResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        SignatureCancellationRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new CancelSignatureContractRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<SignatureCancellationResponse>.Failure(422, errors)
            : null;
    }

}
