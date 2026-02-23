using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Runtime.ExceptionServices;
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

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
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
