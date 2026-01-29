using Domain.Entities;
using Domain.Interfaces;
using FakeApiInfrastructure.Collections;
using InternalHttpClientInfrastructure.Services;



//using InternalHttpClientBuilder;
using Microsoft.Extensions.Logging;

namespace FakeApiInfrastructure.Queries;

public class FakeApiQueryInfra : IExampleTitleQuery
{
    private readonly HttpClientBuilder _httpClient;
    private readonly ILogger<FakeApiQueryInfra> _logger;

    public FakeApiQueryInfra(HttpClientBuilder httpClient, ILogger<FakeApiQueryInfra> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    async public Task<ExampleTitleEntity> GetAsync(int value = 1, CancellationToken ct = default)
    {
        var response = await _httpClient.WithBaseUrl("https://jsonplaceholder.typicode.com")
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
        var response = await _httpClient.WithBaseUrl("https://fakestoreapi.com")
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
