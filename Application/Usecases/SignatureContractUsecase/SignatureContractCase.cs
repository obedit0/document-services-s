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

        //var channelConfig = awsit chanelQuery.getConfigurationById(idCanal: header.ChannelIdentity);
        //ValidateAsync =

        var keynuaRequest = MapToDomain(request);
        string idKeynua = await _keynuaClient.CreateContractAsync(keynuaRequest, ct);

        //var now = DateTimeOffset.UtcNow.ToOffset(_utcOffset);


        //await _repository.InsertAsync(entity, ct);
        var hoola = new CreateSignatureContractResponse { IdFirma = idKeynua };
        return EasyResult<CreateSignatureContractResponse>.Success(hoola);
    }
    
    private static OrdenFirma MapToDomain(CreateSignatureContractRequest request)
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

        return entity;
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
            await _keynuaClient.CancelContractAsync(orden.Id, ct);
        }
        catch (Exception)
        {
            return EasyResult<CancelSignatureContractResponse>.Failure(502, new List<ValidationResultAdapter>());
        }

        await _repository.UpdateStatusAsync(orden.Id, EstadoFirma.CANCELADO, ct);

        return EasyResult<CancelSignatureContractResponse>.Success(new CancelSignatureContractResponse
        {
            Message = "Orden de firma cancelada exitosamente.",
            Estado = EstadoFirma.CANCELADO.ToString()
        });
    }
}
