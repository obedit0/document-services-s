using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Ports;
using Domain.Entities.Internals;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.SignatureStatus;

[Route("document-services-s/signature/status")]
[ApiController]
public class SignatureStatusController : ControllerBase
{
    private const string API_ENDPOINT = "/document-services-s/signature/status";

    private readonly IGetSignatureStatusPort _statusPort;
    private readonly IErrorInternalPort _errorPort;
    private readonly ILogger<SignatureStatusController> _logger;

    public SignatureStatusController(
        ILogger<SignatureStatusController> logger,
        IGetSignatureStatusPort statusPort,
        IErrorInternalPort errorPort)
    {
        _logger = logger;
        _statusPort = statusPort;
        _errorPort = errorPort;
    }

    [HttpGet("retrieve")]
    public async Task<IActionResult> Retrieve(
        [FromHeader] SignatureHeaderRequest header,
        [FromQuery] GetSignatureStatusRequest query)
    {
        try
        {
            var result = await _statusPort.ExecuteAsync(header, query);

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
        catch (Exception ex)
        {
            var tracer = Regex.Replace(ex.StackTrace ?? string.Empty, @"\sat\s(.*?)\sin\s", string.Empty).Trim();
            _logger.LogError("Message=[{Message}] TraceId=[{Headers}] StackTrace={Trace}",
                ex.Message, LoggerMapperHelper.ToString(header), tracer);

            await _errorPort.SaveAsync(new MicroserviceErrorEntity
            {
                ErrorCode = "12005",
                ChannelId = 11,
                Endpoint = $"{API_ENDPOINT}/retrieve",
                MessageIdentification = header.MessageIdentification,
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                StackTrace = tracer,
                Message = ex.Message
            });

            return StatusCode(500, tracer);
        }
    }
}
