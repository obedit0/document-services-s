using Application.Internals.Adapters;
using Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Threading;
using SystemAPI.Handlers.ArifyAuthorizer;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.ExampleBian;

[Route("service-domain-s/v1/example2-behavior-qualifier")]
[ApiController]
public class BianExampleController : ControllerBase
{
    private readonly IExamplePort _exampleUsecase;
    private readonly ILogger<BianExampleController> _logger;

    public BianExampleController(ILogger<BianExampleController> logger, IExamplePort exampleUsecase)
    {   
        _logger = logger;
        _exampleUsecase = exampleUsecase;
    }

    [HttpGet("retrieve")]
    public async Task<IActionResult> Register(
        [FromHeader(Name = "x-device-identifier")] string? deviceIdentifier,
        [FromHeader(Name = "x-message-identifier")] string? messageIdentifier,
        [FromHeader(Name = "x-channel-identifier")] string? channelIdentifier,
        CancellationToken ct = default
    )
    {
        var headers = new TraceIdentifierAdapter
        {
            ChannelIdentifier = channelIdentifier,
            DeviceIdentifier = deviceIdentifier,
            MessageIdentifier = messageIdentifier
        };

        try
        {
            var result = await _exampleUsecase.ShowExampleAsync(headers, ct);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("TraceId=[{Headers}] Validation=[{ValidationErrors}]", LoggerMapperHelper.ToString(headers), LoggerMapperHelper.ToString(result.ValidationValues.FirstOrDefault()!));                
                return StatusCode(result.Status, EasyBianResponseHelper.WarningResponse(result.ValidationValues));                
            }

            if (result.Status == 204)
            {
                _logger.LogWarning("TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
                return NoContent();
            }

            return Ok(EasyBianResponseHelper.SuccessResponse(result.SuccessValue!));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Operation cancelled by client TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
            return StatusCode(499, EasyBianResponseHelper.ErrorResponse("499", "Client Closed Request"));
        }
        catch (Exception ex)
        {
            var tracer = Regex.Replace(ex.StackTrace ?? "", @"\sat\s(.*?)\sin\s", string.Empty).Trim();
            _logger.LogError("Message=[{Message}] TraceId=[{Headers}] StackTrace={Trace}", ex.Message, LoggerMapperHelper.ToString(headers), tracer);

            return StatusCode(500, EasyBianResponseHelper.ErrorResponse());
        }
    }

    [Authorize(Policy = "ReadScope")] // validate by JWT claim
    [HttpGet("execute")]
    public async Task<IActionResult> ExecuteProtected(
        [FromHeader(Name = "x-device-identifier")] string? deviceIdentifier,
        [FromHeader(Name = "x-message-identifier")] string? messageIdentifier,
        [FromHeader(Name = "x-channel-identifier")] string? channelIdentifier,
        CancellationToken ct = default
    )
    {
        var headers = new TraceIdentifierAdapter
        {
            ChannelIdentifier = channelIdentifier,
            DeviceIdentifier = deviceIdentifier,
            MessageIdentifier = messageIdentifier
        };

        try
        {
            var result = await _exampleUsecase.ExecuteExampleTwoAsync(headers);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("TraceId=[{Headers}] Validation=[{ValidationErrors}]", LoggerMapperHelper.ToString(headers), LoggerMapperHelper.ToString(result.ValidationValues.FirstOrDefault()!));                
                return StatusCode(result.Status, EasyBianResponseHelper.WarningResponse(result.ValidationValues));                
            }

            if (result.Status == 204)
            {
                _logger.LogWarning("TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
                return NoContent();
            }

            return Ok(EasyBianResponseHelper.SuccessResponse(result.SuccessValue!));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Operation cancelled by client TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
            return StatusCode(499, EasyBianResponseHelper.ErrorResponse("499", "Client Closed Request"));
        }
        catch (Exception ex)
        {
            var tracer = Regex.Replace(ex.StackTrace ?? "", @"\sat\s(.*?)\sin\s", string.Empty).Trim();
            _logger.LogError("Message=[{Message}] TraceId=[{Headers}] StackTrace={Trace}", ex.Message, LoggerMapperHelper.ToString(headers), tracer);

            return StatusCode(500, EasyBianResponseHelper.ErrorResponse());
        }
    }

    [ArifyAuthorize("WriteExample")] // Validate Scope by x-scope HEADER  
    [HttpPost("create")]
    public async Task<IActionResult> CreateProtected(
       [FromHeader(Name = "x-device-identifier")] string? deviceIdentifier,
       [FromHeader(Name = "x-message-identifier")] string? messageIdentifier,
       [FromHeader(Name = "x-channel-identifier")] string? channelIdentifier,
       [FromHeader(Name = "x-scope")] string? scope,
       CancellationToken ct = default
   )
    {
        var headers = new TraceIdentifierAdapter
        {
            ChannelIdentifier = channelIdentifier,
            DeviceIdentifier = deviceIdentifier,
            MessageIdentifier = messageIdentifier
        };

        try
        {
            var result = await _exampleUsecase.ShowExampleAsync(headers);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("TraceId=[{Headers}] Validation=[{ValidationErrors}]", LoggerMapperHelper.ToString(headers), LoggerMapperHelper.ToString(result.ValidationValues.FirstOrDefault()!));
                return StatusCode(result.Status, EasyBianResponseHelper.WarningResponse(result.ValidationValues));
            }

            if (result.Status == 204)
            {
                _logger.LogWarning("TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
                return NoContent();
            }

            return Ok(EasyBianResponseHelper.SuccessResponse(result.SuccessValue!));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("Operation cancelled by client TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            var tracer = Regex.Replace(ex.StackTrace ?? "", @"\sat\s(.*?)\sin\s", string.Empty).Trim();
            _logger.LogError("Message=[{Message}] TraceId=[{Headers}] StackTrace={Trace}", ex.Message, LoggerMapperHelper.ToString(headers), tracer);

            return StatusCode(500, EasyBianResponseHelper.ErrorResponse());
        }
    }
}
