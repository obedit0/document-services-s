using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Adapters;
using Application.Adapters;
using Application.Internals.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Catalogs;
using Domain.Entities.Client;
using Domain.Entities.Config;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;
using System.Threading.Tasks;
using Channel = Domain.Enums.Channel;

namespace Application.Usecases.SignatureContractUsecase;

public class SignatureContractCase : ISignatureContractPort
{
    private readonly IOrdenFirmaRepository _repository;
    private readonly IChannelConfigRepository _channelConfigRepository;
    private readonly IKeynuaContractClient _keynuaClient;
    private readonly IParametroFirmaRepository _paramRepository;

    public SignatureContractCase(
        IOrdenFirmaRepository repository, 
        IChannelConfigRepository channelConfigRepository,
        IKeynuaContractClient keynuaClient,
        IParametroFirmaRepository paramRepository)
    {
        _repository = repository;
        _channelConfigRepository = channelConfigRepository;
        _keynuaClient = keynuaClient;
        _paramRepository = paramRepository;
    }

    public async Task<EasyResult<CreateSignatureContractResponse>> CreateAsync(SignatureHeaderRequest header, CreateSignatureContractRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = ParseChannel(header.Channel);
        var channelConfig = await _channelConfigRepository.GetByChannelIdAsync((int)canal, ct);
        
        var configValidation = ValidateChannelConfig(channelConfig, request);
        if (configValidation != null)
            return configValidation;

        var existingOrder = await _repository.GetByKeywordAndChannelAsync(request.Keyword!, (int)canal, ct);
        if (existingOrder != null)
            return EasyResult<CreateSignatureContractResponse>.Failure(409, [Error("21008", "Orden de firma ya existe", "Keyword")]);

        OrdenFirma ordenFirma = MapToDomain(request, header);
        if (ordenFirma.Pagare)
        {
            var count = await _repository.GetCountByKeywordAndChannelAsync(request.Keyword!, (int)canal);
            ordenFirma.CreditNumber = int.Parse(((int)canal).ToString() + request.Keyword!.ToString() + count.ToString());
        }
        ordenFirma.IdOrdenProveedor = await _keynuaClient.CreateContractAsync(ordenFirma, ct);
        ordenFirma.IdFirma = await _repository.InsertAsync(ordenFirma, ct);

        return EasyResult<CreateSignatureContractResponse>.Success(MapToResponse(ordenFirma));
    }

    private static EasyResult<CreateSignatureContractResponse>? ValidateChannelConfig(
        ChannelEntity? channelConfig, 
        CreateSignatureContractRequest request)
    {
        var error = GetChannelConfigError(channelConfig, request);
        return error is null ? null : EasyResult<CreateSignatureContractResponse>.Failure(400, [error]);
    }

    private static ValidationResultAdapter? GetChannelConfigError(
        ChannelEntity? config, 
        CreateSignatureContractRequest request)
    {
        if (config is null)
            return Error("21020", "Canal no configurado", "Canal");

        if (!config.Enabled)
            return Error("21021", "Canal deshabilitado", "Canal");

        var upload = config.DocumentsUpload;
        if (upload is null) return null;

        if (request.HoraExpiracion is not null && request.HoraExpiracion.Value.Date != DateTime.Today)
            return Error("21023", "Fecha de expiracion no valida", "HoraExpiracion");

        if (upload.AllowedWindow is not null && request.HoraExpiracion is not null && !IsWithinTimeWindow(upload.AllowedWindow, request.HoraExpiracion.Value))
            return Error("21023", FormatTimeWindowMessage(upload.AllowedWindow), "HoraExpiracion");

        if (request.Documentos is null) return null;

        var docCount = request.Documentos.Count;
        if (docCount > upload.MaxDocuments)
            return Error("21022", $"Documentos ({docCount}) excede l�mite ({upload.MaxDocuments})", "Documentos");

        var totalBytes = request.Documentos.Sum(d => d.Size);
        if (totalBytes > upload.MaxTotalBytes)
            return Error("21024", $"Tama�o ({totalBytes / 1048576} MB) excede l�mite ({upload.MaxTotalBytes / 1048576} MB)", "Documentos");

        return null;
    }

    private static bool IsWithinTimeWindow(AllowedWindowConfig window, DateTime expiration)
    {
        if (expiration.Date != DateTime.Today)
            return false;

        var time = expiration.TimeOfDay;
        var from = TimeSpan.FromMinutes(window.FromMin);
        var to = TimeSpan.FromMinutes(window.ToMin);
        
        return from <= to 
            ? time >= from && time <= to 
            : time >= from || time <= to;
    }

    private static string FormatTimeWindowMessage(AllowedWindowConfig window) =>
        $"Fuera de horario ({window.FromMin / 60:00}:{window.FromMin % 60:00} - {window.ToMin / 60:00}:{window.ToMin % 60:00})";

    private static ValidationResultAdapter Error(string code, string message, string field) =>
        new() { Code = code, Message = message, Field = field };


    private static OrdenFirma MapToDomain(CreateSignatureContractRequest request, SignatureHeaderRequest header)
    {
        var firmantes = request.Clientes
            ?.Select(cliente =>
            {
                var identity = int.TryParse(cliente.IdCliente, out var id) ? id : (int?)null;
                var contact = string.IsNullOrWhiteSpace(cliente.Email) && string.IsNullOrWhiteSpace(cliente.Telefono)
                    ? null
                    : new ContactEntity
                    {
                        Email = cliente.Email,
                        PhoneNumber = cliente.Telefono
                    };

                var identityDocument = string.IsNullOrWhiteSpace(cliente.NumeroDocumento)
                    ? null
                    : new IdentityDocumentEntity
                    {
                        Number = cliente.NumeroDocumento
                    };

                return new NaturalClientEntity
                {
                    Identity = identity,
                    FullName = cliente.NombreCompleto,
                    Contact = contact,
                    IdentityDocument = identityDocument
                };
            })
            .ToList();

        var documentos = request.Documentos
            ?.Select(documento => new Documento
            {
                Name = documento.Name!,
                OwnerClients = documento.OwnerClientId ?? [],
                S3KeyOriginal = documento.S3KeyOriginal!
            })
            .ToList();


        var canal = ParseChannel(header.Channel);

        var ordenFirma = new OrdenFirma
        {
            Referencia = request.Referencia ?? string.Empty,
            Keyword = request.Keyword ?? string.Empty,
            Titulo = request.Titulo ?? string.Empty,
            Descripcion = request.Descripcion ?? string.Empty,
            Canal = canal,
            HoraExpiracion = request.HoraExpiracion!.Value,
            FirmaEnTodosDocumentos = request.FirmaEnTodoDocumentos,
            IdTiposNotificacion = request.IdTiposNotificacion!,
            Pagare = request.Pagare,
            Clientes = firmantes,
            Documentos = documentos,
            Estado = EstadoFirma.PENDIENTE,
            FechaCreacion = DateTime.UtcNow.AddHours(-5),
            FechaActualizacion = DateTime.UtcNow.AddHours(-5),
            Historico = new List<HistoricoEvento>
            {
                new HistoricoEvento
                {
                    FechaEvento = DateTime.UtcNow.AddHours(-5),
                    Fuente = "API",
                    EstadoNuevo = EstadoFirma.PENDIENTE
                }
            }
        };

        return ordenFirma;
    }

    private static CreateSignatureContractResponse MapToResponse(OrdenFirma entity)
    {
        return new CreateSignatureContractResponse
        {
            IdFirma = entity.IdFirma,
            Referencia = entity.Referencia,
            Estado = entity.Estado.ToString(),
            FechaCreacion = entity.FechaCreacion,
            FechaActualizacion = entity.FechaActualizacion
        };
    }

    private static Channel ParseChannel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Channel.Ventanilla;

        if (int.TryParse(value, out var numeric) && Enum.IsDefined(typeof(Channel), numeric))
            return (Channel)numeric;

        return Enum.TryParse<Channel>(value, true, out var channel)
            ? channel
            : Channel.Ventanilla;
    }
    private async Task<EasyResult<CreateSignatureContractResponse>?> ValidateAsync(
    SignatureHeaderRequest header,
    CreateSignatureContractRequest request)
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
            ? EasyResult<CreateSignatureContractResponse>.Failure(422, errors)
            : null;
    }

    public async Task<EasyResult<CancelSignatureContractResponse>> CancelAsync(SignatureHeaderRequest header, CancelSignatureContractRequest request, CancellationToken ct = default)
    {
        var validationErrors = await FluentValidationExecutor.ExecuteAsync(request, new CancelSignatureContractRequestValidator(), ct);
        if (validationErrors.Any())
        {
            return EasyResult<CancelSignatureContractResponse>.Failure(422, validationErrors);
        }

        var orden = await _repository.GetByLegacyReferencesAsync(request.IdCanal!.Value, request.IdCanalTransaccion!.Value, ct);
        if (orden is null)
        {
            return EasyResult<CancelSignatureContractResponse>.Failure(404, [new() { Code = "21101", Message = MessageCatalog.GetErrorByCode(21101) }]);
        }

        var configHorario = await _paramRepository.ObtenerConfiguracionAsync(request.IdCanal.Value, "horaLimiteValidacion", ct);
        var (EsVencido, MensajeError) = ValidarHorarioVencido(orden.FechaCreacion, configHorario);

        if (EsVencido)
        {
            if (orden.Estado != EstadoFirma.EXPIRADO)
            {
                await _repository.UpdateStatusAsync(orden.IdFirma, EstadoFirma.EXPIRADO, ct);
            }

            var mensajeError = MensajeError ?? MessageCatalog.GetErrorByCode(21102);
            return EasyResult<CancelSignatureContractResponse>.Failure(400, [new() { Code = "21102", Message = mensajeError }]);
        }

        switch (orden.Estado)
        {
            case EstadoFirma.PENDIENTE:
            case EstadoFirma.COMPLETADO:
                // CONTINUAR EL FLUJO DE CANCELACIÓN
                break;
            default:
                return EasyResult<CancelSignatureContractResponse>.Failure(400, [new() { Code = "21106", Message = MessageCatalog.GetErrorByCode(21106, orden.Estado.ToString()) }]);
        }

        await _keynuaClient.CancelContractAsync(orden.IdOrdenProveedor, ct);
        
        await _repository.UpdateStatusAsync(orden.IdOrdenProveedor, EstadoFirma.CANCELADO, ct);

        return EasyResult<CancelSignatureContractResponse>.Success(new CancelSignatureContractResponse
        {
            Message = "Orden de firma cancelada exitosamente.",
            Estado = EstadoFirma.CANCELADO.ToString()
        });
    }

    private static (bool EsVencido, string? MensajeError) ValidarHorarioVencido(DateTimeOffset fechaCreacion, ParametroFirma? config)
    {
        int horaLimite = config?.Hora ?? 19;
        int minutoLimite = config?.Minuto ?? 0;
        string timeZoneId = config?.ZonaHoraria ?? "America/Lima";
        string mensajeBase = config?.Descripcion ?? "El horario ha vencido. Hora límite: ";

        try
        {
            TimeZoneInfo timeZone;
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                timeZone = TimeZoneInfo.CreateCustomTimeZone("PeruFallback", TimeSpan.FromHours(-5), "Peru Time", "Peru Time");
            }

            var ahoraEnZona = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            var creacionEnZona = TimeZoneInfo.ConvertTime(fechaCreacion, timeZone);

            if (ahoraEnZona.Date > creacionEnZona.Date)
            {
                return (true, $"{mensajeBase} {horaLimite}:{minutoLimite:00}");
            }

            if (ahoraEnZona.Date == creacionEnZona.Date)
            {
                var tiempoLimite = new TimeSpan(horaLimite, minutoLimite, 0);
                if (ahoraEnZona.TimeOfDay > tiempoLimite)
                {
                    return (true, $"{mensajeBase} {horaLimite}:{minutoLimite:00}");
                }
            }

            return (false, null);
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    public async Task<EasyResult<GetSignatureDocumentStatusResponse>> GetDocumentStatusAsync(GetSignatureDocumentStatusRequest request, CancellationToken ct = default)
    {
        var validationErrors = await FluentValidationExecutor.ExecuteAsync(request, new GetSignatureDocumentStatusValidator());
        if (validationErrors.Any()) return EasyResult<GetSignatureDocumentStatusResponse>.Failure(422, validationErrors);

        var orden = await _repository.GetByLegacyReferencesAsync(request.IdCanal!.Value, request.IdCanalTransaccion!.Value, ct);
        if (orden is null) return EasyResult<GetSignatureDocumentStatusResponse>.Failure(404, [new() { Code = "21101", Message = MessageCatalog.GetErrorByCode(21101) }]);

        var response = new GetSignatureDocumentStatusResponse
        {
            IdFirma = orden.IdFirma,
            Estado = orden.Estado.ToString(),
            IdOrdenProveedor = orden.IdOrdenProveedor ?? string.Empty,
            Documentos = []
        };

        bool validS3 = orden.Documentos.Count != 0 && orden.Documentos.All(d => !string.IsNullOrEmpty(d.S3KeyFirmado) && d.S3KeyFirmadoExpiresAt.HasValue && d.S3KeyFirmadoExpiresAt.Value > DateTime.UtcNow);

        bool validProviderCache = orden.Documentos.Count != 0 && orden.Documentos.All(d => !string.IsNullOrEmpty(d.ProviderKeyFirmado) && d.ProviderKeyFirmadoExpiresAt.HasValue && d.ProviderKeyFirmadoExpiresAt.Value > DateTime.UtcNow);

        if (validS3)
        {
            response.Action = "USE_S3";
            response.Documentos = [.. orden.Documentos.Select(d => new DocumentStatusDto { Nombre = d.Name, Tipo = "PDF", S3Key = d.S3KeyFirmado, Url = null})];
        }
        else if (validProviderCache)
        {
            response.Action = "USE_CACHE";
            response.Documentos = [.. orden.Documentos.Select(d => new DocumentStatusDto { Nombre = d.Name, Tipo = "PDF", Url = d.ProviderKeyFirmado, S3Key = null })];
        }
        else
        {
            response.Action = "FETCH_PROVIDER";
        }

        return EasyResult<GetSignatureDocumentStatusResponse>.Success(response);
    }

}