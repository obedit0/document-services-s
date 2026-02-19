using Domain.Entities;
using Domain.Interfaces;
using FakeApiInfrastructure.Collections;
using InternalHttpClientInfrastructure.Services;



//using InternalHttpClientBuilder;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace FakeApiInfrastructure.Queries;

public class FakeApiQueryInfra : IExampleTitleQuery
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FakeApiQueryInfra> _logger;

    public FakeApiQueryInfra(IHttpClientFactory httpClientFactory, ILogger<FakeApiQueryInfra> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    async public Task<ExampleTitleEntity> GetAsync(int value = 1, CancellationToken ct = default)
    {
        var httpClient = new HttpClientBuilder(_httpClientFactory, _logger);
        var response = await httpClient.WithBaseUrl("https://jsonplaceholder.typicode.com")
            .WithEndpoint($"todos/{value}")
            .GetAsync<ApiExampleCollection>(ct);

        if (!response.IsSuccess) return new ExampleTitleEntity();

        return new ExampleTitleEntity
        {
            Identity = response.Content!.Id,
            Title = response.Content!.Title
        };
    }

    async public Task<ExampleTitleEntity> GetProductAsync(int value = 1, CancellationToken ct = default)
    {
        var httpClient = new HttpClientBuilder(_httpClientFactory, _logger);
        var response = await httpClient.WithBaseUrl("https://fakestoreapi.com")
            .WithEndpoint($"products/{value}")
            .GetAsync<ApiExampleTwoCollection>(ct);

        if (!response.IsSuccess) return new ExampleTitleEntity();

        return new ExampleTitleEntity
        {
            Identity = response.Content!.Id,
            Title = response.Content!.Description
        };
    }
}
