using Domain.Entities.SignatureContracts;
using Domain.Interfaces;
using InternalHttpClientInfrastructure.Collections;
using InternalHttpClientInfrastructure.Services;
using KeynuaInfrastructure.Collections.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InternalHttpClientInfrastructure.Queries;

public sealed class KeynuaContractClient : IKeynuaContractClient
{
    private readonly HttpClientBuilder _httpClientBuilder;
    private readonly ILogger<KeynuaContractClient> _logger;
    private readonly KeynuaContext _options;

    public KeynuaContractClient(HttpClientBuilder httpClientBuilder, ILogger<KeynuaContractClient> logger, IOptions<KeynuaContext> options)
    {
        _httpClientBuilder = httpClientBuilder;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> CreateContractAsync(OrdenFirma orden, CancellationToken ct = default)
    {
        var payload = BuildKeynuaRequest(orden);

        var response = await _httpClientBuilder
            .WithBaseUrl(_options.BaseUrl)
            .WithEndpoint("contracts/v1")
            .WithHeader("x-api-key", _options.ApiKey)
            .WithHeader("authorization", _options.Authorization)
            .PutAsync<KeynuaContractPropertiesResponse>(payload, ct);

        if (!response.IsSuccess || response.Content is null)
        {
            throw new ArgumentException("Error con la peticion a Keynua");
        }
        if (string.IsNullOrWhiteSpace(response.Content.Id))
        {
            throw new ArgumentException("Error con la peticion a Keynua: respuesta sin Id");
        }

        return response.Content.Id;
    }
    private static KeynuaContractRequest BuildKeynuaRequest(OrdenFirma orden)
    {
        var documents = orden.Documentos?
            .Select(documento => new KeynuaContractDocument
            {
                Name = documento.IdDocumento ?? "documento",
                Base64 = documento.S3KeyOriginal ?? string.Empty
            })
            .ToList() ?? new List<KeynuaContractDocument>();

        var users = new List<KeynuaContractUser>();

        if (orden.Clientes is not null)
        {
            users.AddRange(orden.Clientes.Select(cliente => new KeynuaContractUser
            {
                Name = cliente.NombreCompleto ?? cliente.IdCliente ?? string.Empty,
                Email = cliente.Email,
                Phone = cliente.Telefono,
                Groups = ["signers"]
            }));
        }

        if (orden.Observadores is not null)
        {
            users.AddRange(orden.Observadores.Select(observador => new KeynuaContractUser
            {
                Name = observador.IdObservador ?? string.Empty,
                Email = observador.Email,
                Phone = null,
                Groups = ["signers"]
            }));
        }

        return new KeynuaContractRequest
        {
            Title = orden.Titulo ?? string.Empty,
            Description = orden.Descripcion,
            Reference = orden.Referencia?.ToString() ?? string.Empty, // ajusta si ReferenciaFirma expone .Value
            Documents = documents,
            Users = users,
            AddSignatureOnAllDocs = orden.FirmaEnTodosDocumentos,
            ChosenNotificationOptions = orden.IdTiposNotificacion
        };
    }

    public async Task CancelContractAsync(string keynuaId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keynuaId))
            throw new ArgumentException("El ID de Keynua no puede estar vacío.");

        var response = await _httpClientBuilder
            .WithBaseUrl(_options.BaseUrl)
            .WithEndpoint($"contracts/v1/{keynuaId}")
            .WithHeader("x-api-key", _options.ApiKey)
            .WithHeader("authorization", _options.Authorization)
            .DeleteAsync<object>(ct);

        if (!response.IsSuccess)
        {
            _logger.LogError("Error cancelando en Keynua. Status: {Status}, Content: {Content}", response.StatusCode, response.Content);
            throw new HttpRequestException($"Keynua API Error: {response.StatusCode}");
        }
    }
}


