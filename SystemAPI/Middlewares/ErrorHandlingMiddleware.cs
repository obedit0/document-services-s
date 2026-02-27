using Application.Ports;
using Domain.Entities.Internals;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using SystemAPI.Attributes;
using SystemAPI.Helpers;

namespace SystemAPI.Middlewares;

public sealed class ErrorHandlingMiddleware
{
    private const string DefaultErrorCode = "21098";
    private const string ClientClosedCode = "499";
    private const string ClientClosedMessage = "Client Closed Request";

    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            await SaveErrorAsync(context, ClientClosedCode, ClientClosedMessage, null);
            await WriteErrorAsync(context, 499, ClientClosedCode, ClientClosedMessage, null);
        }
        catch (HttpErrorException ex)
        {
            _logger.LogError(
                ex,
                "Request failed. {Method} {Path} Status={StatusCode} Code={ErrorCode}",
                context.Request.Method,
                context.Request.Path,
                ex.StatusCode,
                ex.ErrorCode);

            await SaveErrorAsync(context, ex.ErrorCode, ex.Message, ex);
            await WriteErrorAsync(context, ex.StatusCode, ex.ErrorCode, ex.Message, ex);
        }
        catch (Exception ex)
        {
            var errorCode = GetDefaultErrorCode(context) ?? DefaultErrorCode;

            _logger.LogError(
                ex,
                "Unhandled exception. {Method} {Path} Code={ErrorCode}",
                context.Request.Method,
                context.Request.Path,
                errorCode);

            await SaveErrorAsync(context, errorCode, ex.Message, ex);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, errorCode, null, ex);
        }
    }

    private static string? GetDefaultErrorCode(HttpContext context)
    {
        return context.GetEndpoint()?.Metadata.GetMetadata<DefaultErrorCodeAttribute>()?.ErrorCode;
    }

    private static bool IsBianResponse(HttpContext context)
    {
        return context.GetEndpoint()?.Metadata.GetMetadata<BianResponseAttribute>() is not null;
    }

    private async Task SaveErrorAsync(HttpContext context, string errorCode, string? message, Exception? ex)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var tracePort = scope.ServiceProvider.GetRequiredService<IMicroserviceTracePort>();

            var traceId = GetHeaderValue(context,
                "messageIdentification",
                "x-message-identifier",
                "message-identification") ?? context.TraceIdentifier;

            var channelId = GetHeaderValue(context,
                "channelIdentification",
                "x-channel-identifier",
                "channel-identification") ?? "unknown";

            var deviceId = GetHeaderValue(context,
                "deviceIdentification",
                "x-device-identifier",
                "device-identification") ?? "unknown";

            var endpoint = $"{context.Request.Path}{context.Request.QueryString}";
            var payload = await ReadRequestBodyAsync(context);
            var headers = SerializeHeaders(context);

            var trace = new MicroserviceErrorTraceEntity
            {
                Identity = errorCode,
                TraceId = traceId,
                ChannelId = channelId,
                DeviceId = deviceId,
                RequestUrl = endpoint,
                RequestHeader = headers,
                RequestPayload = payload,
                ErrorMessage = message ?? ex?.Message ?? string.Empty,
                ErrorStackTrace = ex?.ToString() ?? string.Empty,
                Datetime = DateTime.UtcNow.AddHours(-5),
                IsResolved = false
            };

            await tracePort.SaveErrorAsync(trace, CancellationToken.None);
        }
        catch (Exception saveEx)
        {
            _logger.LogError(saveEx, "Failed to persist error trace for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
    }

    private static string? GetHeaderValue(HttpContext context, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (context.Request.Headers.TryGetValue(key, out var value))
            {
                var headerValue = value.ToString();
                if (!string.IsNullOrWhiteSpace(headerValue))
                    return headerValue;
            }
        }

        return null;
    }

    private static string? SerializeHeaders(HttpContext context)
    {
        if (context.Request.Headers.Count == 0)
            return null;

        var headers = context.Request.Headers
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        return JsonSerializer.Serialize(headers);
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpContext context)
    {
        if (!context.Request.Body.CanSeek)
            return null;

        if (context.Request.Body.Length == 0)
            return null;

        context.Request.Body.Position = 0;
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        return string.IsNullOrWhiteSpace(body) ? null : body;
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string errorCode, string? message, Exception? ex)
    {
        if (context.Response.HasStarted)
        {
            if (ex is not null)
                ExceptionDispatchInfo.Capture(ex).Throw();

            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        if (IsBianResponse(context))
        {
            var response = message is null
                ? EasyBianResponseHelper.ErrorResponse()
                : EasyBianResponseHelper.ErrorResponse(errorCode, message);

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        var nonBianResponse = message is null
            ? EasyResponseHelper.ErrorResponse(errorCode)
            : EasyResponseHelper.ErrorResponse(errorCode, message);

        await context.Response.WriteAsJsonAsync(nonBianResponse);
    }
}
