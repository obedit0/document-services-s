using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Entities.Client;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Usecases.SignatureStatusUsecase;

public sealed class GetSignatureStatusCase : IGetSignatureStatusPort
{
    private readonly IOrdenFirmaRepository _repository;
    private readonly IKeynuaContractClient _keynuaClient;

    public GetSignatureStatusCase(IOrdenFirmaRepository repository, IKeynuaContractClient keynuaClient)
    {
        _repository = repository;
        _keynuaClient = keynuaClient;
    }

    public async Task<EasyResult<GetSignatureStatusResponse>> ExecuteAsync(
        SignatureHeaderRequest header,
        GetSignatureStatusRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await ValidateAsync(header, request);
        if (validationResult != null)
            return validationResult;

        var canal = (Channel)request.IdCanal!.Value;
        var keyword = request.IdCanalTransaccion!.Value.ToString();

        var ordenFirma = await _repository.GetByKeywordAndChannelAsync(keyword, (int)canal, ct);
        if (ordenFirma is null)
        {
            return EasyResult<GetSignatureStatusResponse>.Failure(404,
            [
                new() { Code = "21010", Message = "Orden de firma no encontrada", Field = "IdCanalTransaccion" }
            ]);
        }

        if (string.IsNullOrWhiteSpace(ordenFirma.IdOrdenProveedor))
        {
            return EasyResult<GetSignatureStatusResponse>.Failure(404,
            [
                new() { Code = "21010", Message = "Orden de firma sin id de proveedor", Field = "IdOrdenKeynua" }
            ]);
        }

        string? status;
        try
        {
            status = await _keynuaClient.GetContractStatusAsync(
                ordenFirma.IdOrdenProveedor,
                canal,
                header.MessageIdentification!,
                ct);
        }
        catch (Exception ex)
        {
            return EasyResult<GetSignatureStatusResponse>.Failure(502,
            [
                new() { Code = "21098", Message = ex.Message, Field = "Keynua" }
            ]);
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return EasyResult<GetSignatureStatusResponse>.Failure(502,
            [
                new() { Code = "21098", Message = "Estado de contrato no disponible", Field = "Keynua" }
            ]);
        }

        var normalizedStatus = NormalizeStatus(status);

        ordenFirma.Estado = MapEstadoFirma(normalizedStatus);
        ordenFirma.FechaActualizacion = DateTime.UtcNow.AddHours(-5);

        await _repository.UpdateAsync(ordenFirma, ct);

        var clientes = ordenFirma.Clientes ?? [];

        var response = new GetSignatureStatusResponse
        {
            IdCanal = request.IdCanal.Value,
            IdCanalTransaction = request.IdCanalTransaccion.Value,
            IdOrdenKeynua = ordenFirma.IdOrdenProveedor,
            CEstadoGeneral = normalizedStatus,
            CEstadoCanal = normalizedStatus,
            ArrFirmantes = clientes.Select(cliente => new SignatureStatusSignerResponse
            {
                CDNI = cliente.IdentityDocument?.Number ?? cliente.Identity?.ToString(),
                CNombreCompleto = BuildFullName(cliente),
                CCorreo = cliente.Contact?.Email,
                CCelular = cliente.Contact?.PhoneNumber,
                NEstado = MapSignerStatus(normalizedStatus)
            }).ToList()
        };

        return EasyResult<GetSignatureStatusResponse>.Success(response);
    }

    private async Task<EasyResult<GetSignatureStatusResponse>?> ValidateAsync(
        SignatureHeaderRequest header,
        GetSignatureStatusRequest request)
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
            ? EasyResult<GetSignatureStatusResponse>.Failure(422, errors)
            : null;
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToLowerInvariant();
    }

    private static EstadoFirma MapEstadoFirma(string status)
    {
        return status switch
        {
            "pending_input" => EstadoFirma.PENDIENTE,
            "pending" => EstadoFirma.PENDIENTE,
            "working" => EstadoFirma.EN_PROCESO,
            "pending_approval" => EstadoFirma.EN_PROCESO,
            "contract_approval" => EstadoFirma.EN_PROCESO,
            "in_progress" => EstadoFirma.EN_PROCESO,
            "done" => EstadoFirma.COMPLETADO,
            "deleted" => EstadoFirma.ANULADO,
            "canceled" => EstadoFirma.ANULADO,
            "cancelled" => EstadoFirma.ANULADO,
            "expired" => EstadoFirma.EXPIRADO,
            "error" => EstadoFirma.ERROR,
            _ => EstadoFirma.ERROR
        };
    }

    private static int MapSignerStatus(string status)
    {
        return status switch
        {
            "pending_input" => 1,
            "pending" => 1,
            "working" => 2,
            "pending_approval" => 2,
            "contract_approval" => 2,
            "in_progress" => 2,
            "error" => 3,
            "deleted" => 3,
            "done" => 4,
            _ => 0
        };
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
