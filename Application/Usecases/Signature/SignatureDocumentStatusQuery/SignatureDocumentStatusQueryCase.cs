using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Application.Usecases.Signature;
using Domain.Catalogs;
using Domain.Interfaces;

namespace Application.Usecases.Signature.SignatureDocumentStatusQuery;

public class SignatureDocumentStatusQueryCase : IGetSignatureDocumentStatusPort
{
    private readonly ISignatureQuery _signatureQuery;

    public SignatureDocumentStatusQueryCase(ISignatureQuery signatureQuery)
    {
        _signatureQuery = signatureQuery;
    }

    public async Task<EasyResult<GetSignatureDocumentStatusResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        GetSignatureDocumentStatusRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification);
        if (canal is null)
            return EasyResult<GetSignatureDocumentStatusResponse>.Failure(422, [new ValidationResultAdapter("21008", "Canal no definido", "Canal")]);

        var orden = await _signatureQuery.GetByKeywordAndChannelAsync(
            request.Keyword!.Value,
            (int)canal,
            ct);

        if (orden is null)
        {
            return EasyResult<GetSignatureDocumentStatusResponse>.Failure(404,
            [
                new ValidationResultAdapter("21101", MessageCatalog.GetErrorByCode(21101), null)
            ]);
        }

        var response = new GetSignatureDocumentStatusResponse
        {
            IdFirma = orden.SignatureId,
            Estado = orden.Status.ToString(),
            IdOrdenProveedor = orden.ProviderIdentity ?? string.Empty,
            Documentos = []
        };

        var validS3 = orden.Documents.Count != 0 &&
            orden.Documents.All(d => !string.IsNullOrEmpty(d.S3KeyFirmado) &&
                                      d.S3KeyFirmadoExpiresAt.HasValue &&
                                      d.S3KeyFirmadoExpiresAt.Value > DateTime.UtcNow);

        var validProviderCache = orden.Documents.Count != 0 &&
            orden.Documents.All(d => !string.IsNullOrEmpty(d.ProviderKeyFirmado) &&
                                      d.ProviderKeyFirmadoExpiresAt.HasValue &&
                                      d.ProviderKeyFirmadoExpiresAt.Value > DateTime.UtcNow);

        if (validS3)
        {
            response.Action = "USE_S3";
            response.Documentos = [.. orden.Documents.Select(d =>
                new DocumentStatusDto
                {
                    Nombre = d.Name,
                    Tipo = "PDF",
                    S3Key = d.S3KeyFirmado
                })];
        }
        else if (validProviderCache)
        {
            response.Action = "USE_CACHE";
            response.Documentos = [.. orden.Documents.Select(d =>
                new DocumentStatusDto
                {
                    Nombre = d.Name,
                    Tipo = "PDF",
                    Url = d.ProviderKeyFirmado
                })];
        }
        else
        {
            response.Action = "FETCH_PROVIDER";
        }

        return EasyResult<GetSignatureDocumentStatusResponse>.Success(response);
    }

    private async Task<EasyResult<GetSignatureDocumentStatusResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        GetSignatureDocumentStatusRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new GetSignatureDocumentStatusRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<GetSignatureDocumentStatusResponse>.Failure(422, errors)
            : null;
    }
}
