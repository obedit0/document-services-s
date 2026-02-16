using Application.Adapters;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;
using Channel = Domain.Enums.Channel;

namespace Application.Usecases.SignatureContractUsecase;

public class SignatureContractCase : ISignatureContractPort
{
    private readonly IOrdenFirmaRepository _repository;
    private readonly IKeynuaContractClient _keynuaClient;

    public SignatureContractCase(IOrdenFirmaRepository repository, IKeynuaContractClient keynuaClient)
    {
        _repository = repository;
        _keynuaClient = keynuaClient;
    }

    public async Task<EasyResult<CreateSignatureContractResponse>> CreateAsync(SignatureHeaderRequest header, CreateSignatureContractRequest request, CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        //var existing = await _repository.GetByKeywordAsync(request.Keyword!, string channel, ct);
        //if (existing is not null)
        //{
        //    return EasyResult<CreateSignatureContractResponse>.Success(MapToResponse(existing));
        //}

        //var channelConfig = awsit chanelQuery.getConfigurationById(idCanal: header.ChannelIdentity);
        //ValidateAsync =

        var ordenFirma = MapToDomain(request);
        ordenFirma.IdOrdenProveedor = await _keynuaClient.CreateContractAsync(ordenFirma, ct);

        //var now = DateTimeOffset.UtcNow.ToOffset(_utcOffset);


        ordenFirma.IdFirma = await _repository.InsertAsync(ordenFirma, ct);
        return EasyResult<CreateSignatureContractResponse>.Success(MapToResponse(ordenFirma));
    }
    
    private static OrdenFirma MapToDomain(CreateSignatureContractRequest request)
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
                IdDocumento = documento.IdDocumento!,
                TipoDocumento = documento.TipoDocumento!,
                NombreDocumento = documento.IdDocumento!,
                OwnerClient = documento.OwnerClienteId!,
                S3KeyOriginal = documento.S3KeyOriginal!
            })
            .ToList();

        var canal = ParseChannel(request.Canal);
        var keyword = request.Keyword;
        var horaExpiracion = request.HoraExpiracion?.UtcDateTime ?? DateTime.UtcNow.AddHours(24);

        var ordenFirma = new OrdenFirma
        {
            Referencia = request.Referencia ?? string.Empty,
            Keyword = keyword,
            Titulo = request.Titulo ?? string.Empty,
            Descripcion = request.Descripcion ?? string.Empty,
            Canal = canal,
            HoraExpiracion = horaExpiracion,
            FirmaEnTodosDocumentos = request.FirmaEnTodoDocumentos,
            IdTiposNotificacion = request.IdTiposNotificacion,
            Pagare = request.Pagare,
            Clientes = firmantes,
            Documentos = documentos,
            Estado = EstadoFirma.PENDIENTE,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow,
            Historico = new List<HistoricoEvento>
            {
                new HistoricoEvento
                {
                    FechaEvento = DateTime.UtcNow,
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
}
