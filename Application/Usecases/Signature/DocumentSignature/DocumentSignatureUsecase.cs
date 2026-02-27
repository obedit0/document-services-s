using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Mapper.DocumentSignatureMapper;
using Application.Ports;
using Application.Usecases.Signature;
using Domain.Entities.Config;
using Domain.Entities.SignatureContracts;
using Domain.Interfaces;

namespace Application.Usecases.Signature.SignatureContractCreation;

public class DocumentSignatureUsecase : ISignatureContractPort
{
    private readonly ISignatureQuery _signatureQuery;
    private readonly ISignatureCommand _signatureCommand;
    private readonly IChannelQuery _channelConfigRepository;
    private readonly ISignatureContractCommand _contractCommand;

    public DocumentSignatureUsecase(
        ISignatureQuery signatureQuery,
        ISignatureCommand signatureCommand,
        IChannelQuery channelConfigRepository,
        ISignatureContractCommand contractCommand)
    {
        _signatureQuery = signatureQuery;
        _signatureCommand = signatureCommand;
        _channelConfigRepository = channelConfigRepository;
        _contractCommand = contractCommand;
    }

    public async Task<EasyResult<SignatureResponse>> CreateAsync(SignatureHeaderRequest header, SignatureRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = SignatureUsecaseHelper.ParseChannel(header.ChannelIdentification);
        if (canal is null)
            return EasyResult<SignatureResponse>.Failure(409, [new ValidationResultAdapter("21008", "Canal no definido", "Canal")]);

        var channelConfig = await _channelConfigRepository.GetByChannelIdAsync((int)canal, ct);

        var configValidation = ValidateChannelConfig(channelConfig, request);
        if (configValidation != null)
            return configValidation;

        var existingOrder = await _signatureQuery.GetByKeywordAndChannelAsync(request.Keyword!.Value, (int)canal, ct);
        if (existingOrder != null)
            return EasyResult<SignatureResponse>.Failure(409, [new ValidationResultAdapter("21008", $"Orden {request.Keyword} de firma ya existe", "Keyword")]);

        SignatureEntity ordenFirma = DocumentSignatureMapper.MapToDomain(request, header);
        if (ordenFirma.PromissoryNote)
        {
            var count = await _signatureQuery.CountByKeywordAndChannelAsync(request.Keyword!.Value, (int)canal);
            ordenFirma.CreditNumber = int.Parse(((int)canal).ToString() + request.Keyword!.ToString() + count.ToString());
        }
        ordenFirma.ProviderIdentity = await _contractCommand.CreateAsync(ordenFirma, ct);
        ordenFirma.SignatureId = await _signatureCommand.InsertAsync(ordenFirma, ct);

        return EasyResult<SignatureResponse>.Success(DocumentSignatureMapper.MapToResponse(ordenFirma));
    }

    private static EasyResult<SignatureResponse>? ValidateChannelConfig(
        ChannelEntity? channelConfig, 
        SignatureRequest request)
    {
        var error = GetChannelConfigError(channelConfig, request);
        return error is null ? null : EasyResult<SignatureResponse>.Failure(400, [error]);
    }

    private static ValidationResultAdapter? GetChannelConfigError(
        ChannelEntity? config, 
        SignatureRequest request)
    {
        if (config is null)
            return new ValidationResultAdapter("21020", "Canal no configurado", "Canal");

        if (!config.Enabled)
            return new ValidationResultAdapter("21021", "Canal deshabilitado", "Canal");

        var upload = config.DocumentsUpload;
        if (upload is null) return null;

        if (request.HoraExpiracion is not null && request.HoraExpiracion.Value.Date != DateTime.Today)
            return new ValidationResultAdapter("21023", "Fecha de expiracion no valida", "HoraExpiracion");

        var uploadWindow = config.UploadTimeWindow;
        if (uploadWindow is not null && request.HoraExpiracion is not null && !SignatureUsecaseHelper.IsWithinTimeWindow(uploadWindow, request.HoraExpiracion.Value))
            return new ValidationResultAdapter("21023", "Fuera de Horario", "HoraExpiracion");

        if (request.Documentos is null) return null;

        var docCount = request.Documentos.Count;
        if (docCount > upload.MaxDocuments)
            return new ValidationResultAdapter("21022", $"Documentos ({docCount}) excede limite ({upload.MaxDocuments})", "Documentos");

        var totalBytes = request.Documentos.Sum(d => d.Size);
        if (totalBytes > upload.MaxTotalBytes)
            return new ValidationResultAdapter("21024", $"Tamanio ({totalBytes / 1048576} MB) excede limite ({upload.MaxTotalBytes / 1048576} MB)", "Documentos");

        return null;
    }

    private async Task<EasyResult<SignatureResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        SignatureRequest request)
    {
        var validationTasks = new[]
        {
            FluentValidationExecutor.ExecuteAsync(
                header, new SignatureHeaderRequestValidator()),
            FluentValidationExecutor.ExecuteAsync(
                request, new CreateSignatureContractRequestValidator())
        };

        var results = await Task.WhenAll(validationTasks);
        var errors = results.SelectMany(e => e).ToList();

        return errors.Count != 0
            ? EasyResult<SignatureResponse>.Failure(422, errors)
            : null;
    }
}
