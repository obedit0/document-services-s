using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Application.Usecases.Signature;
using Domain.Entities.Client;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.Signature.SignatureStatusQuery;

public sealed class DocumentSignatureInquiryUsecase : IGetSignatureStatusPort
{
    private readonly ISignatureQuery _signatureQuery;
    private readonly ISignatureCommand _signatureCommand;
    private readonly ISignatureContractQuery _contractQuery;

    public DocumentSignatureInquiryUsecase(
        ISignatureQuery signatureQuery,
        ISignatureCommand signatureCommand,
        ISignatureContractQuery contractQuery)
    {
        _signatureQuery = signatureQuery;
        _signatureCommand = signatureCommand;
        _contractQuery = contractQuery;
    }

    public async Task<EasyResult<SignatureInquiryResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        SignatureInquiryRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification);
        if (canal is null)
            return EasyResult<SignatureInquiryResponse>.Failure(422, [new ValidationResultAdapter("21008", "Canal no definido", "Canal")]);

        var keyword = request.Keyword!.Value;

        var ordenFirma = await _signatureQuery.GetByKeywordAndChannelAsync(keyword, (int)canal, ct);
        if (ordenFirma is null)
        {
            return EasyResult<SignatureInquiryResponse>.Failure(404,
            [
                new ValidationResultAdapter("21010", "Orden de firma no encontrada", "Keyword")
            ]);
        }

        if (string.IsNullOrWhiteSpace(ordenFirma.ProviderIdentity))
        {
            return EasyResult<SignatureInquiryResponse>.Failure(404,
            [
                new ValidationResultAdapter("21010", "Orden de firma sin id de proveedor", "IdOrdenKeynua")
            ]);
        }

        var status = await _contractQuery.GetStatusAsync(ordenFirma.ProviderIdentity, (ChannelEnum)canal, header.MessageIdentification!, ct);

        ordenFirma.Status = status;
        ordenFirma.UpdatedAt = DateTime.UtcNow.AddHours(-5);

        await _signatureCommand.UpdateAsync(ordenFirma, ct);

        var clients = ordenFirma.Clients ?? [];

        var response = new SignatureInquiryResponse
        {
            Keyword = request.Keyword.Value,
            IdOrdenKeynua = ordenFirma.ProviderIdentity,
            CEstadoGeneral = status.ToString(),
            CEstadoCanal = status.ToString(),
            ArrFirmantes = clients.Select(cliente => new SignatureStatusSignerResponse
            {
                CDNI = cliente.IdentityDocument?.Number ?? cliente.Identity?.ToString(),
                CNombreCompleto = BuildFullName(cliente),
                CCorreo = cliente.Contact?.Email,
                CCelular = cliente.Contact?.PhoneNumber,
                NEstado = (int)status
            }).ToList()
        };

        return EasyResult<SignatureInquiryResponse>.Success(response);
    }

    private async Task<EasyResult<SignatureInquiryResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        SignatureInquiryRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new GetSignatureStatusRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<SignatureInquiryResponse>.Failure(422, errors)
            : null;
    }

    private static string? BuildFullName(NaturalClientEntity cliente)
    {
        if (!string.IsNullOrWhiteSpace(cliente.FullName))
            return cliente.FullName;

        var parts = new[]
        {
            cliente.GivenName,
            cliente.PaternalLastName,
            cliente.MaternalLastName
        };

        var fullName = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
    }
}
