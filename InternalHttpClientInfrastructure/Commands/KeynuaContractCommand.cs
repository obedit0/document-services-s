using Domain.Catalogs;
using Domain.Containers.MemoryEvent;
using Domain.Entities.Client;
using Domain.Entities.SignatureContracts;
using Domain.Exceptions;
using Domain.Interfaces;
using InternalHttpClientInfrastructure.Collections;
using InternalHttpClientInfrastructure.Services;
using KeynuaInfrastructure.Collections.Request;
using KeynuaInfrastructure.Collections.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace InternalHttpClientInfrastructure.Commands;

public sealed class KeynuaContractCommand : ISignatureContractCommand
{
    private const int KeynuaErrorStatus = 502;
    private const string KeynuaErrorCode = "21098";
    private const int InvalidRequestStatus = 400;
    private const string InvalidRequestCode = "21002";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MicroserviceCallMemoryQueue _memQueue;
    private readonly ILogger<KeynuaContractCommand> _logger;
    private readonly KeynuaContext _options;

    public KeynuaContractCommand(
        IHttpClientFactory httpClientFactory,
        ILogger<KeynuaContractCommand> logger,
        IOptions<KeynuaContext> options,
        MicroserviceCallMemoryQueue memQueue)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
        _memQueue = memQueue;
    }

    public async Task<string> CreateAsync(SignatureEntity orden, CancellationToken ct = default)
    {
        var payload = BuildKeynuaRequest(orden);
        var httpClient = new HttpClientBuilder(_httpClientFactory, _logger);
        var response = await httpClient
            .WithBaseUrl(_options.BaseUrl)
            .WithEndpoint("contracts/v1")
            //.WithMemoryQueue(_memQueue, "Crear.orden.firma", orden.Keyword!.ToString())
            .WithHeader("x-api-key", _options.ApiKey)
            .WithHeader("authorization", _options.Authorization)
            .PutAsync<KeynuaContractPropertiesResponse>(payload, ct);

        int status = response.StatusCode;
        if (status > 299 && status < 500)
        {
            throw new ClientErrorException((int)HttpStatusCode.UnprocessableEntity, "21098", MessageCatalog.GetErrorByCode(21098, _options.BaseUrl));
        }
        if (status > 499)
        {
            throw new ServerErrorException((int)HttpStatusCode.ServiceUnavailable, "21107", MessageCatalog.GetErrorByCode(21107, _options.BaseUrl));
        }

        if (response.Content is null)
        {
            throw new ServerErrorException(KeynuaErrorStatus, KeynuaErrorCode, "Error con la peticion a Keynua");
        }
        if (string.IsNullOrWhiteSpace(response.Content.Id))
        {
            throw new ServerErrorException(KeynuaErrorStatus, KeynuaErrorCode, "Error con la peticion a Keynua: respuesta sin Id");
        }

        return response.Content.Id;
    }

    public async Task CancelAsync(string contractId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contractId))
        {
            throw new ClientErrorException(
                InvalidRequestStatus,
                InvalidRequestCode,
                $"Cannot be empty: {nameof(contractId)}");
        }

        var response = await new HttpClientBuilder(_httpClientFactory, _logger)
            .WithBaseUrl(_options.BaseUrl)
            .WithEndpoint($"contracts/v1/{contractId}")
            .WithHeader("x-api-key", _options.ApiKey)
            .WithHeader("authorization", _options.Authorization)
            .DeleteAsync<object>(ct);

        if (!response.IsSuccess)
        {
            throw new ServerErrorException(
                KeynuaErrorStatus,
                KeynuaErrorCode,
                "Error con la peticion a Keynua: cancelar contrato");
        }
    }

    private KeynuaSignatureRequest BuildKeynuaRequest(SignatureEntity orden)
    {
        var documents = (orden.Documents ?? new List<DocumentEntity>())
            .Select(documento => new KeynuaDocumentRequest
            {
                Name = documento.Name,
                Base64 = documento.S3KeyOriginal,
                RefId = documento.Name
            })
            .ToList();

        var documentsByOwner = (orden.Documents ?? new List<DocumentEntity>())
            .SelectMany(documento => documento.OwnerClients.Select(owner => new { Owner = owner, DocName = documento.Name }))
            .GroupBy(x => x.Owner)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(x => x.DocName).ToList());

        var allDocumentRefs = documents.Select(documento => documento.RefId).ToList();

        var users = new List<KeynuaUserRequest>();

        if (orden.Clients is not null)
        {
            foreach (var cliente in orden.Clients)
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

        var expirationDatetime = DateTime.SpecifyKind(orden.ExpirationTime, DateTimeKind.Utc).ToUniversalTime();
        var notificationOptions = orden.NotificationTypeIds ?? new List<string>();
        var cavaliData = BuildCavaliData(orden);

        return new KeynuaSignatureRequest
        {
            Title = orden.Title,
            Description = orden.Description,
            Reference = orden.Keyword.ToString(),
            Documents = documents,
            TemplateId = orden.PromissoryNote ? "dnice-cavali" : "andes-peru-dni",
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
                    AddSignatureOnAllDocs = false
                },
                ChosenNotificationOptions = notificationOptions,
                CavaliData = cavaliData
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

    private KeynuaCavaliDataRequest? BuildCavaliData(SignatureEntity orden)
    {
        if (!orden.PromissoryNote)
            return null;

        var primaryClient = orden.Clients?.FirstOrDefault();

        return new KeynuaCavaliDataRequest
        {
            Client = primaryClient,
            UniqueCode = primaryClient?.Identity,
            ClientName = primaryClient is null ? null : GetClientName(primaryClient),
            Domicile = GetClientDomicile(primaryClient),
            IssuedDate = DateTime.Today.ToString("yyyy-MM-dd"),
            Banking = _options.Banking,
            Product = _options.Product,
            CreditNumber = orden.CreditNumber
        };
    }

    private static string? GetClientDomicile(ClientEntity? client)
    {
        var address = client?.Addresses?.FirstOrDefault();
        if (address is null)
            return null;

        var parts = new[]
        {
            address.Name,
            address.Street,
            address.Number,
            address.Reference,
            address.PostalCode
        };

        var domicile = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(domicile) ? null : domicile;
    }
}
