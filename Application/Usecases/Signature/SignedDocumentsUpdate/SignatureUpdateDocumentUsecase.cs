using Application.Adapters.Common;
using Application.Adapters.SignatureContracts.UpdateDocuments;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Application.Usecases.Signature;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.Signature.SignedDocumentsUpdate;

public class SignedDocumentsUpdateCase : IUpdateSignedDocumentsPort
{
    private readonly ISignatureQuery _signatureQuery;
    private readonly ISignatureCommand _signatureCommand;

    public SignedDocumentsUpdateCase(ISignatureQuery signatureQuery, ISignatureCommand signatureCommand)
    {
        _signatureQuery = signatureQuery;
        _signatureCommand = signatureCommand;
    }

    public async Task<EasyResult<SignatureUpdateDocumentResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SignatureUpdateDocumentRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification);
        if (canal is null)
            return EasyResult<SignatureUpdateDocumentResponse>.Failure(409, [new ValidationResultAdapter("21008", "Canal no definido", "Canal")]);

        var keyword = int.TryParse(request.IdFirma, out var kw) ? kw : 0;

        var ordenFirma = await _signatureQuery.GetByKeywordAndChannelAsync(keyword, (int)canal, ct);
        if (ordenFirma == null)
            return EasyResult<SignatureUpdateDocumentResponse>.Failure(409, [new ValidationResultAdapter("21008", "Orden de firma No existe", "Keyword")]);

        var updatedCount = MapSignedDocuments(ordenFirma, request.Documents!);

        ordenFirma.UpdatedAt = DateTime.UtcNow.AddHours(-5);
        ordenFirma.History ??= [];
        ordenFirma.History.Add(new HistoryEntity
        {
            EventDate = DateTime.UtcNow.AddHours(-5),
            Source = "API-WEBHOOK",
            PreviousStatus = ordenFirma.Status,
            NewStatus = SignatureStatus.COMPLETADO,
            Reason = "Documentos firmados recibidos"
        });
        ordenFirma.Status = SignatureStatus.COMPLETADO;

        await _signatureCommand.UpdateAsync(ordenFirma, ct);

        return EasyResult<SignatureUpdateDocumentResponse>.Success(new SignatureUpdateDocumentResponse
        {
            IdFirma = ordenFirma.SignatureId,
            Estado = ordenFirma.Status.ToString(),
            FechaActualizacion = ordenFirma.UpdatedAt,
            DocumentosActualizados = updatedCount
        });
    }

    private static int MapSignedDocuments(SignatureEntity ordenFirma, List<SignedDocumentRequest> signedDocuments)
    {
        var updatedCount = 0;
        foreach (var signedDoc in signedDocuments)
        {
            var documento = ordenFirma.Documents
                .FirstOrDefault(d =>
                    string.Equals(d.Name, signedDoc.Name, StringComparison.OrdinalIgnoreCase));

            if (documento is not null)
            {
                documento.S3KeyFirmado = signedDoc.S3KeyFirmado;
                documento.S3KeyFirmadoExpiresAt = signedDoc.UrlExpiresAt;
                documento.FechaFirma = signedDoc.UrlExpiresAt ?? DateTime.UtcNow;
                updatedCount++;
            }
        }
        return updatedCount;
    }

    private async Task<EasyResult<SignatureUpdateDocumentResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        SignatureUpdateDocumentRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new UpdateSignedDocumentsRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<SignatureUpdateDocumentResponse>.Failure(422, errors)
            : null;
    }
}
