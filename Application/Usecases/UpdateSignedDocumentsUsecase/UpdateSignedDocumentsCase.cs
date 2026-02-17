using Application.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.UpdateSignedDocumentsUsecase;

public class UpdateSignedDocumentsCase : IUpdateSignedDocumentsPort
{
    private readonly IOrdenFirmaRepository _repository;

    public UpdateSignedDocumentsCase(IOrdenFirmaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EasyResult<UpdateSignedDocumentsResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        UpdateSignedDocumentsRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var ordenFirma = await _repository.GetByKeywordAsync(request.IdFirma!, ct);
        if (ordenFirma is null)
        {
            return EasyResult<UpdateSignedDocumentsResponse>.Failure(404,
            [
                new() { Code = "21010", Message = "Orden de firma no encontrada", Field = "IdFirma" }
            ]);
        }

        var updatedCount = MapSignedDocuments(ordenFirma, request.Documents!);

        ordenFirma.FechaActualizacion = DateTime.UtcNow.AddHours(-5);
        ordenFirma.Historico ??= [];
        ordenFirma.Historico.Add(new HistoricoEvento
        {
            FechaEvento = DateTime.UtcNow.AddHours(-5),
            Fuente = "API-WEBHOOK",
            EstadoAnterior = ordenFirma.Estado,
            EstadoNuevo = EstadoFirma.COMPLETADO,
            Motivo = "Documentos firmados recibidos"
        });
        ordenFirma.Estado = EstadoFirma.COMPLETADO;

        await _repository.UpdateAsync(ordenFirma, ct);

        return EasyResult<UpdateSignedDocumentsResponse>.Success(new UpdateSignedDocumentsResponse
        {
            IdFirma = ordenFirma.IdFirma,
            Estado = ordenFirma.Estado.ToString(),
            FechaActualizacion = ordenFirma.FechaActualizacion,
            DocumentosActualizados = updatedCount
        });
    }

    private static int MapSignedDocuments(OrdenFirma ordenFirma, List<SignedDocumentRequest> signedDocuments)
    {
        var updatedCount = 0;
        foreach (var signedDoc in signedDocuments)
        {
            var documento = ordenFirma.Documentos
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

    private async Task<EasyResult<UpdateSignedDocumentsResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        UpdateSignedDocumentsRequest request)
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
            ? EasyResult<UpdateSignedDocumentsResponse>.Failure(422, errors)
            : null;
    }
}
