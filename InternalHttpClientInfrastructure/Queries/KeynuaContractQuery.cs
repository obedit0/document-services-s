using Domain.Containers.MemoryEvent;
using Domain.Exceptions;
using Domain.Enums;
using Domain.Interfaces;
using InternalHttpClientInfrastructure.Collections;
using InternalHttpClientInfrastructure.Mappers;
using InternalHttpClientInfrastructure.Services;
using KeynuaInfrastructure.Collections.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using Domain.Catalogs;

namespace InternalHttpClientInfrastructure.Queries;

public sealed class KeynuaContractQuery : ISignatureContractQuery
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MicroserviceCallMemoryQueue _memQueue;
    private readonly ILogger<KeynuaContractQuery> _logger;
    private readonly KeynuaContext _options;

    public KeynuaContractQuery(
        IHttpClientFactory httpClientFactory,
        ILogger<KeynuaContractQuery> logger,
        IOptions<KeynuaContext> options,
        MicroserviceCallMemoryQueue memQueue)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value;
        _memQueue = memQueue;
    }

    public async Task<SignatureStatus> GetStatusAsync(string contractId, ChannelEnum channel, string messageIdentity, CancellationToken ct = default)
    {
        var httpClient = new HttpClientBuilder(_httpClientFactory, _logger);
        var response = await httpClient
            .WithBaseUrl(_options.BaseUrl)
            .WithEndpoint($"contracts/v1/{contractId}")
            .WithMemoryQueue(_memQueue, "Consultar.estado.firma", contractId)
            .WithHeader("x-api-key", _options.ApiKey)
            .WithHeader("authorization", _options.Authorization)
            .GetAsync<KeynuaContractPropertiesResponse>(ct);

        int status = response.StatusCode;
        if (status > 299 && status < 500)
        {
            throw new ClientErrorException((int)HttpStatusCode.UnprocessableEntity, "21107", MessageCatalog.GetErrorByCode(21107,"Keynua"));
        }
        if (status > 499)
        {
            throw new ServerErrorException((int)HttpStatusCode.ServiceUnavailable, "21107", MessageCatalog.GetErrorByCode(21107, "Keynua"));
        }

        var normalizedStatus = response.Content.Status?.Trim().ToLowerInvariant() ?? string.Empty;
        return KeynuaStatusMapper.MapSignatureStatus(normalizedStatus);
    }

    public async Task<bool> HealthcheckAsync(CancellationToken ct = default)
    {
        try
        {
            var httpClient = new HttpClientBuilder(_httpClientFactory, _logger);
            var response = await httpClient
                .WithBaseUrl(_options.BaseUrl!)
                .WithHeader("x-api-key", _options.ApiKey!)
                .WithHeader("authorization", _options.Authorization!)
                .GetAsync<object>(ct);

            return response.StatusCode > 0 && response.StatusCode < 500;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Keynua healthcheck failed");
            return false;
        }
    }
}
