using Application.Internals.Adapters;
using Application.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using SystemAPI.Attributes;
using SystemAPI.Handlers.ArifyAuthorizer;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.ExampleBian;

[Route("service-domain-s/v1/example2-behavior-qualifier")]
[ApiController]
[BianResponse]
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

        var result = await _exampleUsecase.ExecuteExampleTwoAsync(headers, ct);

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
}
