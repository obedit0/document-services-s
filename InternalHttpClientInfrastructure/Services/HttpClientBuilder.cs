using Domain.Containers.MemoryEvent;
using InternalHttpClientInfrastructure.Collections;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

/* ********************************************************************************************************          
# * Copyright � 2025 Arify Labs - All rights reserved.   
# * 
# * Info                  : Http Client Builder.
# *
# * By                    : Victor Jhampier Caxi Maquera
# * Email/Mobile/Phone    : victorjhampier@gmail.com | 968991714
# *
# * Creation date         : 01/01/2026
# * 
# **********************************************************************************************************/

namespace InternalHttpClientInfrastructure.Services;

public sealed class HttpClientBuilder
{
    private readonly IHttpClientFactory _factory;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private string _clientName = "ArifyClient";
    private string? _baseUrl;
    private string? _endpoint;
    private readonly Dictionary<string, string> _headers = new();
    private readonly Dictionary<string, string> _query = new();
    private TimeSpan? _timeout;
    private MicroserviceCallMemoryQueue _queue;

    public HttpClientBuilder(IHttpClientFactory factory, ILogger logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };        
    }

    public HttpClientBuilder WithMemoryQueue(MicroserviceCallMemoryQueue queue)
    {
        _queue = queue;
        return this;
    }

    public HttpClientBuilder WithClient(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _clientName = name;
        return this;
    }

    public HttpClientBuilder WithBaseUrl(string baseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        _baseUrl = baseUrl.TrimEnd('/');
        return this;
    }

    public HttpClientBuilder WithEndpoint(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        _endpoint = endpoint.TrimStart('/');
        return this;
    }

    public HttpClientBuilder WithHeader(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        _headers[key] = value;
        return this;
    }

    public HttpClientBuilder WithBearerToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return WithHeader("Authorization", $"Bearer {token}");
    }

    public HttpClientBuilder WithAuthorization(string scheme, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return WithHeader("Authorization", $"{scheme} {token}");
    }

    public HttpClientBuilder WithQuery(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        _query[key] = value;
        return this;
    }

    public HttpClientBuilder WithTimeout(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
        _timeout = timeout;
        return this;
    }

    private Uri BuildUri()
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
            throw new InvalidOperationException("Base URL must be set before making requests");

        var path = string.IsNullOrWhiteSpace(_endpoint) 
            ? _baseUrl 
            : $"{_baseUrl}/{_endpoint}";

        if (_query.Count == 0)
            return new Uri(path);

        var uriBuilder = new UriBuilder(path);
        var queryString = string.Join("&", _query.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        uriBuilder.Query = queryString;

        return uriBuilder.Uri;
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, object? body = null)
    {
        var uri = BuildUri();
        var request = new HttpRequestMessage(method, uri);

        // Add headers
        foreach (var header in _headers)
        {
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                _logger.LogWarning("Failed to add header {HeaderKey} with value {HeaderValue}", header.Key, header.Value);
            }
        }

        // Add content if body is provided
        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    public async Task<HttpResponseResult<T>> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(HttpMethod.Get, null, cancellationToken);
    }

    public async Task<HttpResponseResult<T>> PostAsync<T>(object? body = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(HttpMethod.Post, body, cancellationToken);
    }

    public async Task<HttpResponseResult<T>> PutAsync<T>(object? body = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(HttpMethod.Put, body, cancellationToken);
    }

    public async Task<HttpResponseResult<T>> DeleteAsync<T>(CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(HttpMethod.Delete, null, cancellationToken);
    }

    private async Task<HttpResponseResult<T>> ExecuteRequestAsync<T>(HttpMethod method, object? body, CancellationToken cancellationToken)
    {
        var client = _factory.CreateClient(_clientName);
        
        if (_timeout.HasValue)
            client.Timeout = _timeout.Value;

        using var request = BuildRequest(method, body);
        
        try
        {
            _logger.LogDebug("Executing {Method} request to {Uri}", method, request.RequestUri);
            
            using var response = await client.SendAsync(request, cancellationToken);
            return await BuildResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("{Message} < HTTP request failed for {Method} {Uri}", ex.Message, method, request.RequestUri);
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError("{Message} < Request timeout for {Method} {Uri}", ex.Message, method, request.RequestUri);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(" {Message} < Unexpected error during {Method} request to {Uri}", ex.Message, method, request.RequestUri);
            throw;
        }
    }

    private async Task<HttpResponseResult<T>> BuildResponseAsync<T>(HttpResponseMessage response)
    {
        T? content = default;
        var requestUri = response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
        bool isSuccess = false;

        try
        {            
            if ((int)response.StatusCode == 200)
            {
                content = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                isSuccess = content is not null;
            }            
            else
            {
                var headers = response.Headers
                .Concat(response.Content.Headers)
                .ToDictionary(h => h.Key, h => string.Join(",", h.Value));
                _logger.LogWarning("Response >> {StatusCode} {Uri}\nHeaders={headers} Content={content}", (int)response.StatusCode, requestUri, headers, response.Content.ToString());
            }   
        }
        catch (JsonException ex)
        {
            _logger.LogError("{Message} >> Failed to deserialize response >> {StatusCode} {Uri} >> Content may not match expected type {Type}\n{content}", ex.Message, (int)response.StatusCode, requestUri, typeof(T).Name, response.Content.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError("{Message} >> Unexpected error while reading response >> {StatusCode} {Uri}\n{content}", ex.Message, (int)response.StatusCode, requestUri, response.Content.ToString());
        }        

        return new HttpResponseResult<T>(
            (int)response.StatusCode,
            isSuccess,
            content,
            requestUri
        );
    }
}
