using Application.Adapters.Common;
using Application.Adapters.SignatureContracts.DocumentSignatureCompletion;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Application.Usecases.Signature;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.Signature.ProviderDocumentsUpdate;

public class DocumentSignatureCompletionUsecase : IUpdateProviderDocumentsPort
{
    private readonly ISignatureQuery _signatureQuery;
    private readonly ISignatureCommand _signatureCommand;

    public DocumentSignatureCompletionUsecase(ISignatureQuery signatureQuery, ISignatureCommand signatureCommand)
    {
        _signatureQuery = signatureQuery;
        _signatureCommand = signatureCommand;
    }

    public async Task<EasyResult<SignatureCompletionResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SignatureCompletionRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification);
        if (canal is null)
            return EasyResult<SignatureCompletionResponse>.Failure(409, [new ValidationResultAdapter("21008", "Canal no definido", "Canal")]);

        var keyword = int.TryParse(request.IdFirma, out var kw) ? kw : 0;

        var ordenFirma = await _signatureQuery.GetByKeywordAndChannelAsync(keyword, (int)canal, ct);
        if (ordenFirma == null)
            return EasyResult<SignatureCompletionResponse>.Failure(409, [new ValidationResultAdapter("21008", "Orden de firma No existe", "Keyword")]);

        var updatedCount = MapProviderDocuments(ordenFirma, request.Documents!);

        var now = DateTime.UtcNow.AddHours(-5);
        var newHistory = new HistoryEntity
        {
            EventDate = now,
            Source = "API-PROVIDER-WEBHOOK",
            PreviousStatus = ordenFirma.Status,
            NewStatus = SignatureStatus.COMPLETADO,
            Reason = "Documentos firmados con provider key recibidos"
        };
        ordenFirma.Status = SignatureStatus.COMPLETADO;
        ordenFirma.UpdatedAt = now;

        await _signatureCommand.UpdateDocumentsAsync(
            ordenFirma.SignatureId,
            ordenFirma.Documents,
            ordenFirma.Status,
            newHistory,
            ct);

        return EasyResult<SignatureCompletionResponse>.Success(new SignatureCompletionResponse
        {
            IdFirma = ordenFirma.SignatureId,
            Estado = ordenFirma.Status.ToString(),
            FechaActualizacion = ordenFirma.UpdatedAt,
            DocumentosActualizados = updatedCount
        });
    }

    private static int MapProviderDocuments(SignatureEntity ordenFirma, List<ProviderDocumentRequest> providerDocuments)
    {
        var updatedCount = 0;
        foreach (var providerDoc in providerDocuments)
        {
            var documento = ordenFirma.Documents
                .FirstOrDefault(d =>
                    string.Equals(d.Name, providerDoc.Name, StringComparison.OrdinalIgnoreCase));

            if (documento is not null)
            {
                documento.ProviderKeyFirmado = providerDoc.ProviderKeyFirmado;
                documento.ProviderKeyFirmadoExpiresAt = providerDoc.UrlExpiresAt;
                documento.FechaFirma = providerDoc.UrlExpiresAt ?? DateTime.UtcNow;
                updatedCount++;
            }
        }
        return updatedCount;
    }

    private async Task<EasyResult<SignatureCompletionResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        SignatureCompletionRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new UpdateProviderDocumentsRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<SignatureCompletionResponse>.Failure(422, errors)
            : null;
    }
}
