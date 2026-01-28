using System.Text.RegularExpressions;
using Application.Internals.Adapters;
using Application.Ports;
using Microsoft.AspNetCore.Mvc;
using SystemAPI.Helpers;
using SystemAPI.Models.Internals;

namespace SystemAPI.Controllers.ExampleBasic;

[Route("service-domain-s/v1/example-behavior-qualifier")]
[ApiController]
public class ExampleController : ControllerBase
{
    private readonly IExamplePort _exampleUsecase;
    private readonly ILogger<ExampleController> _logger;

    public ExampleController(ILogger<ExampleController> logger, IExamplePort exampleUsecase)
    {   
        _logger = logger;
        _exampleUsecase = exampleUsecase;        
    }

    [HttpGet("retrieve")]
    async public Task<IActionResult> Get(
        [FromHeader(Name = "x-device-identifier")] string? deviceIdentifier,
        [FromHeader(Name = "x-message-identifier")] string? messageIdentifier,
        [FromHeader(Name = "x-channel-identifier")] string? channelIdentifier
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
                return StatusCode(result.Status, EasyResponseHelper.WarningResponse(result.ValidationValues));                
            }

            if (result.Status == 204)
            {
                _logger.LogWarning("TraceId=[{Headers}]", LoggerMapperHelper.ToString(headers));
                return NoContent();
            }

            return Ok(EasyResponseHelper.SuccessResponse(result.SuccessValue!));
        }
        catch (Exception ex)
        {
            var tracer = Regex.Replace(ex.StackTrace ?? "", @"\sat\s(.*?)\sin\s", string.Empty).Trim();
            _logger.LogError("Message=[{Message}] TraceId=[{Headers}] StackTrace={Trace}", ex.Message, LoggerMapperHelper.ToString(headers), tracer);

            return StatusCode(500, EasyResponseHelper.ErrorResponse("11033"));            
        }
    }
}
