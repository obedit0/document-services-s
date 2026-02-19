using Application.Adapters.Common;
using Application.Adapters.UpdateDocuments;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.UpdateProviderDocumentsUsecase;

public class UpdateProviderDocumentsCase : IUpdateProviderDocumentsPort
{
    private readonly IOrdenFirmaRepository _repository;

    public UpdateProviderDocumentsCase(IOrdenFirmaRepository repository)
    {
        _repository = repository;
    }

    public async Task<EasyResult<UpdateProviderDocumentsResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        UpdateProviderDocumentsRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var ordenFirma = await _repository.GetByKeywordAsync(request.IdFirma!, ct);
        if (ordenFirma is null)
        {
            return EasyResult<UpdateProviderDocumentsResponse>.Failure(404,
            [
                new() { Code = "21010", Message = "Orden de firma no encontrada", Field = "IdFirma" }
            ]);
        }

        var updatedCount = MapProviderDocuments(ordenFirma, request.Documents!);

        ordenFirma.FechaActualizacion = DateTime.UtcNow.AddHours(-5);
        ordenFirma.Historico ??= [];
        ordenFirma.Historico.Add(new HistoricoEvento
        {
            FechaEvento = DateTime.UtcNow.AddHours(-5),
            Fuente = "API-PROVIDER-WEBHOOK",
            EstadoAnterior = ordenFirma.Estado,
            EstadoNuevo = EstadoFirma.COMPLETADO,
            Motivo = "Documentos firmados con provider key recibidos"
        });
        ordenFirma.Estado = EstadoFirma.COMPLETADO;

        await _repository.UpdateAsync(ordenFirma, ct);

        return EasyResult<UpdateProviderDocumentsResponse>.Success(new UpdateProviderDocumentsResponse
        {
            IdFirma = ordenFirma.IdFirma,
            Estado = ordenFirma.Estado.ToString(),
            FechaActualizacion = ordenFirma.FechaActualizacion,
            DocumentosActualizados = updatedCount
        });
    }

    private static int MapProviderDocuments(OrdenFirma ordenFirma, List<ProviderDocumentRequest> providerDocuments)
    {
        var updatedCount = 0;
        foreach (var providerDoc in providerDocuments)
        {
            var documento = ordenFirma.Documentos
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

    private async Task<EasyResult<UpdateProviderDocumentsResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        UpdateProviderDocumentsRequest request)
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
            ? EasyResult<UpdateProviderDocumentsResponse>.Failure(422, errors)
            : null;
    }
}
