using Application.Adapters;
using Application.Internals.Executors;
using Application.Internals.Adapters;
using Application.Ports;
using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Catalogs;
using System.Text.Json;

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

        //var existing = await _repository.GetByReferenciaAsync(request.Referencia!, ct);
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
            .Cast<ClientEntity>()
            .ToList() ?? new List<ClientEntity>();

        var documentos = request.Documentos
            ?.Select(documento => new Documento
            {
                IdDocumento = documento.IdDocumento ?? string.Empty,
                TipoDocumento = documento.TipoDocumento ?? string.Empty,
                NombreDocumento = documento.IdDocumento ?? string.Empty,
                OwnerClient = documento.OwnerClienteId ?? string.Empty,
                S3KeyOriginal = documento.S3KeyOriginal ?? string.Empty
            })
            .ToList() ?? new List<Documento>();

        var canal = ParseChannel(request.Canal);
        var horaExpiracion = request.HoraExpiracion?.UtcDateTime ?? DateTime.UtcNow.AddHours(-5).AddHours(24);

        var ordenFirma = new OrdenFirma
        {
            IdFirma = Guid.NewGuid().ToString("N"),
            Referencia = request.Referencia ?? string.Empty,
            Keyword = request.Keyword,
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
        var validationErrors = await FluentValidationExecutor.ExecuteAsync(request, new CancelSignatureContractRequestValidator());
        if (validationErrors.Any())
        {
            return EasyResult<CancelSignatureContractResponse>.Failure(422, validationErrors);
        }

        var orden = await _repository.GetByLegacyReferencesAsync(request.IdCanal!.Value, request.IdCanalTransaccion!.Value, ct);

        if (orden is null)
        {
            return EasyResult<CancelSignatureContractResponse>.Failure(404, new List<ValidationResultAdapter>());
        }

        if (orden.Estado == EstadoFirma.CANCELADO)
        {
            return EasyResult<CancelSignatureContractResponse>.Success(new CancelSignatureContractResponse
            {
                Message = "Orden de firma cancelada exitosamente.",
                Estado = orden.Estado.ToString()
            });
        }

        if (orden.Estado == EstadoFirma.COMPLETADO)
        {
            return EasyResult<CancelSignatureContractResponse>.Success(new CancelSignatureContractResponse
            {
                Message = "La orden ya ha sido firmada y no se puede cancelar.",
                Estado = orden.Estado.ToString()
            });
        }

        try
        {
            await _keynuaClient.CancelContractAsync(orden.IdOrdenProveedor, ct);
        }
        catch (Exception)
        {
            return EasyResult<CancelSignatureContractResponse>.Failure(502, new List<ValidationResultAdapter>());
        }

        await _repository.UpdateStatusAsync(orden.IdOrdenProveedor, EstadoFirma.CANCELADO, ct);

        return EasyResult<CancelSignatureContractResponse>.Success(new CancelSignatureContractResponse
        {
            Message = "Orden de firma cancelada exitosamente.",
            Estado = EstadoFirma.CANCELADO.ToString()
        });
    }
}
