using Application.Adapters.Common;
using Application.Adapters.Sqs;
using Application.Ports;
using Microsoft.AspNetCore.Mvc;
using SystemAPI.Attributes;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.Sqs;

[Route("system-signature-s/v1/sqs")]
[ApiController]
[DefaultErrorCode("21098")]
public class SqsTestController : ControllerBase
{
    private readonly ISqsTestPort _sqsTestPort;
    private readonly ILogger<SqsTestController> _logger;

    public SqsTestController(ILogger<SqsTestController> logger, ISqsTestPort sqsTestPort)
    {
        _logger = logger;
        _sqsTestPort = sqsTestPort;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest(
        [FromHeader] SignatureHeaderRequest header,
        [FromBody] SqsSendTestRequest body)
    {
        var result = await _sqsTestPort.ExecuteAsync(header, body);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("TraceId=[{Headers}] Validation=[{ValidationErrors}]",
                LoggerMapperHelper.ToString(header),
                LoggerMapperHelper.ToString(result.ValidationValues.FirstOrDefault()!));
            return StatusCode(result.Status, EasyResponseHelper.WarningResponse(result.ValidationValues));
        }

        if (result.Status == 204)
        {
            _logger.LogWarning("TraceId=[{Headers}]", LoggerMapperHelper.ToString(header));
            return NoContent();
        }

        return Ok(EasyResponseHelper.SuccessResponse(result.SuccessValue!));
    }
}
