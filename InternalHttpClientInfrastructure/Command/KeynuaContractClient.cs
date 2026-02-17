using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Interfaces;
using InternalHttpClientInfrastructure.Collections;
using InternalHttpClientInfrastructure.Services;
using KeynuaInfrastructure.Collections.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

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
        //var options = new JsonSerializerOptions
        //{
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //};
        //string jsonstring = JsonSerializer.Serialize(payload, options);
        //Console.WriteLine(jsonstring);
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
    private KeynuaSignatureRequest BuildKeynuaRequest(OrdenFirma orden)
    {
        var documents = (orden.Documentos ?? new List<Documento>())
            .Select(documento => new KeynuaDocumentRequest
            {
                Name = documento.Name,
                Base64 = documento.S3KeyOriginal,
                RefId = documento.Name
            })
            .ToList();

        var documentsByOwner = (orden.Documentos ?? new List<Documento>())
            .SelectMany(documento => documento.OwnerClients.Select(owner => new { Owner = owner, DocName = documento.Name }))
            .GroupBy(x => x.Owner)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(x => x.DocName).ToList());

        var allDocumentRefs = documents.Select(documento => documento.RefId).ToList();

        var users = new List<KeynuaUserRequest>();

        if (orden.Clientes is not null)
        {
            foreach (var cliente in orden.Clientes)
            {
                var ownerKey = cliente.Identity?.ToString() ?? string.Empty;
                var documentsRefs = documentsByOwner.TryGetValue(ownerKey, out var refs) && refs.Count > 0
                    ? refs
                    : allDocumentRefs;

                var nombreDocumento = cliente.IdentityDocument?.Number ?? string.Empty;
                var prefilledItems = string.IsNullOrWhiteSpace(nombreDocumento)
                    ? new List<KeynuaPrefilledItemRequest>()
                    : new List<KeynuaPrefilledItemRequest>
                    {
                        new KeynuaPrefilledItemRequest
                        {
                            Target = "5",
                            Value = new KeynuaValueRequest { Text = nombreDocumento }
                        }
                    };

                users.Add(new KeynuaUserRequest
                {
                    Name = GetClientName(cliente),
                    Email = cliente.Contact?.Email ?? string.Empty,
                    Phone = NormalizePhone(cliente.Contact?.PhoneNumber),
                    Groups = ["firmantes-1"],
                    DocumentsRefs = documentsRefs,
                    PrefilledItems = prefilledItems
                });
            }
        }

        //if (orden.Observadores is not null)
        //{
        //    users.AddRange(orden.Observadores.Select(observador => new KeynuaUserRequest
        //    {
        //        Name = observador.IdObservador ?? string.Empty,
        //        Email = observador.Email,
        //        Phone = string.Empty,
        //        Groups = ["signers"],
        //        DocumentsRefs = allDocumentRefs,
        //        PrefilledItems = new List<KeynuaPrefilledItemRequest>()
        //    }));
        //}

        var expirationDatetime = DateTime.SpecifyKind(orden.HoraExpiracion, DateTimeKind.Utc).ToUniversalTime();
        var notificationOptions = orden.IdTiposNotificacion ?? new List<string>();

        return new KeynuaSignatureRequest
        {
            Title = orden.Titulo,
            Description = orden.Descripcion,
            Reference = orden.Referencia,
            Documents = documents,
            TemplateId = orden.Pagare ? "dnice-cavali" : "andes-peru-dni",
            ExpirationDatetime = expirationDatetime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            Users = users,
            Flags = new KeynuaFlagsRequest
            {
                RemindersData = new KeynuaRemindersDataRequest
                {
                    Frequency = 120,
                    MaxAttempts = 3
                },
                PDFData = new KeynuaPDFDataRequest
                {
                    AddSignatureOnAllDocs = orden.FirmaEnTodosDocumentos
                },
                ChosenNotificationOptions = notificationOptions
            }
        };
    }

    private static string GetClientName(ClientEntity cliente)
    {
        if (cliente is NaturalClientEntity natural && !string.IsNullOrWhiteSpace(natural.FullName))
            return natural.FullName;

        if (!string.IsNullOrWhiteSpace(cliente.IdentityDocument?.Number))
            return cliente.IdentityDocument!.Number!;

        return cliente.Identity?.ToString() ?? string.Empty;
    }

    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var trimmed = phone.Trim();
        if (trimmed.StartsWith("+", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("51", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        return "51" + trimmed;
    }

}


