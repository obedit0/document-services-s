using Application.Adapters;
using Application.Internals.Executors;
using Application.Internals.Adapters;
using Application.Ports;
using Domain.Entities.SignatureContracts;
using Domain.Interfaces;
using Domain.Catalogs;
using System.Text.Json;

namespace Application.Usecases.SignatureContractUsecase;

public class SignatureContractCase : ISignatureContractPort
{
    private static readonly TimeSpan _utcOffset = TimeSpan.FromHours(-5);
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

        var existing = await _repository.GetByReferenciaAsync(request.Referencia!, ct);
        if (existing is not null)
        {
            return EasyResult<CreateSignatureContractResponse>.Success(MapToResponse(existing));
        }

        var keynuaRequest = BuildKeynuaRequest(request);
        var keynuaResult = await _keynuaClient.CreateContractAsync(keynuaRequest, ct);
        if (!keynuaResult.IsSuccess)
        {
            var validation = new[]
            {
                new ValidationResultAdapter
                {
                    Code = "21098",
                    Message = MessageCatalog.GetErrorByCode(21098),
                    Field = "Keynua"
                }
            };
            var status = keynuaResult.StatusCode <= 0 ? 502 : keynuaResult.StatusCode;
            return EasyResult<CreateSignatureContractResponse>.Failure(status, validation);
        }

        var now = DateTimeOffset.UtcNow.ToOffset(_utcOffset);
        var entity = MapToDomain(request, now, keynuaResult.ProviderId);

        await _repository.InsertAsync(entity, ct);

        return EasyResult<CreateSignatureContractResponse>.Success(MapToResponse(entity));
    }

    private static OrdenFirma MapToDomain(CreateSignatureContractRequest request, DateTimeOffset now, string? providerId)
    {
        var clientes = request.Clientes
            ?.Select(cliente => new Cliente
            {
                IdCliente = cliente.IdCliente!,
                TipoVinculo = cliente.TipoVinculo!,
                NombreCompleto = cliente.NombreCompleto!,
                Email = cliente.Email,
                Telefono = cliente.Telefono
            })
            .ToList() ?? new List<Cliente>();

        var documentos = request.Documentos
            ?.Select(documento => new Documento
            {
                IdDocumento = documento.IdDocumento!,
                TipoDocumento = documento.TipoDocumento!,
                OwnerClienteId = documento.OwnerClienteId!,
                S3KeyOriginal = documento.S3KeyOriginal!,
                HashSha256 = documento.HashSha256,
                S3KeyFirmado = null,
                ProviderKeyFirmado = null,
                FechaFirma = null
            })
            .ToList() ?? new List<Documento>();

        var observadores = request.Observadores
            ?.Select(observador => new Observador
            {
                IdObservador = observador.IdObservador!,
                Email = observador.Email!,
                Rol = observador.Rol
            })
            .ToList();

        var entity = new OrdenFirma
        {
            Id = Guid.NewGuid().ToString("N"),
            Referencia = new ReferenciaFirma(request.Referencia!),
            Proveedor = request.Proveedor!,
            IdOrdenProveedor = providerId,
            Titulo = request.Titulo!,
            Descripcion = request.Descripcion,
            Canal = request.Canal,
            HoraExpiracion = request.HoraExpiracion,
            FirmaEnTodosDocumentos = request.FirmaEnTodosDocumentos,
            IdTiposNotificacion = request.IdTiposNotificacion,
            Clientes = clientes,
            Documentos = documentos,
            Observadores = observadores,
            Estado = EstadoFirma.PENDIENTE,
            FechaCreacion = now,
            FechaActualizacion = now,
            Historico = new List<HistoricoEvento>
            {
                new HistoricoEvento
                {
                    FechaEvento = now,
                    Fuente = "API",
                    EstadoNuevo = EstadoFirma.PENDIENTE
                }
            }
        };

        return entity;
    }

    private static KeynuaContractRequest BuildKeynuaRequest(CreateSignatureContractRequest request)
    {
        var documents = request.Documentos?
            .Select(documento => new KeynuaContractDocument
            {
                Name = documento.IdDocumento ?? "documento",
                Base64 = documento.S3KeyOriginal ?? string.Empty
            })
            .ToList() ?? new List<KeynuaContractDocument>();

        var users = new List<KeynuaContractUser>();

        if (request.Clientes is not null)
        {
            users.AddRange(request.Clientes.Select(cliente => new KeynuaContractUser
            {
                Name = cliente.NombreCompleto ?? cliente.IdCliente ?? string.Empty,
                Email = cliente.Email,
                Phone = cliente.Telefono,
                Groups = ["signers"]
            }));
        }

        if (request.Observadores is not null)
        {
            users.AddRange(request.Observadores.Select(observador => new KeynuaContractUser
            {
                Name = observador.IdObservador ?? string.Empty,
                Email = observador.Email,
                Phone = null,
                Groups = ["signers"]
            }));
        }

        return new KeynuaContractRequest
        {
            Title = request.Titulo ?? string.Empty,
            Description = request.Descripcion,
            Reference = request.Referencia ?? string.Empty,
            Documents = documents,
            Users = users,
            AddSignatureOnAllDocs = request.FirmaEnTodosDocumentos,
            ChosenNotificationOptions = request.IdTiposNotificacion
        };
    }

    private static CreateSignatureContractResponse MapToResponse(OrdenFirma entity)
    {
        return new CreateSignatureContractResponse
        {
            IdFirma = entity.Id,
            Referencia = entity.Referencia.Value,
            Estado = entity.Estado.ToString(),
            FechaCreacion = entity.FechaCreacion,
            FechaActualizacion = entity.FechaActualizacion
        };
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
