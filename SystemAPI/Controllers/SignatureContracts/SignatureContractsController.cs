using Application.Adapters;
using Application.Ports;
using Domain.Entities.Internals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.SignatureContracts;

[Route("system-signature-s/v1/contracts")]
[ApiController]
public class SignatureContractsController : ControllerBase
{
    private const string API_ENDPOINT = "/system-signature-s/v1/contracts";

    private readonly ISignatureContractPort _signaturePort;
    private readonly IErrorInternalPort _errorPort;
    private readonly ILogger<SignatureContractsController> _logger;

    public SignatureContractsController(
        ILogger<SignatureContractsController> logger,
        ISignatureContractPort signaturePort,
        IErrorInternalPort errorPort)
    {
        _logger = logger;
        _signaturePort = signaturePort;
        _errorPort = errorPort;
    }

    //[Authorize(Policy = "WriteScope")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader] SignatureHeaderRequest header,
        [FromBody] CreateSignatureContractRequest body)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(header.MessageIdentification))
            {
                header.MessageIdentification = Request.Headers["x-message-identifier"].FirstOrDefault();
            }

            var result = await _signaturePort.CreateAsync(header, body);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("TraceId=[{Headers}] Validation=[{ValidationErrors}]", LoggerMapperHelper.ToString(header), LoggerMapperHelper.ToString(result.ValidationValues.FirstOrDefault()!));
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
            _logger.LogError("Message=[{Message}] TraceId=[{Headers}] StackTrace={Trace}", ex.Message, LoggerMapperHelper.ToString(header), tracer);

            await _errorPort.SaveAsync(new MicroserviceErrorEntity
            {
                ErrorCode = "12001",
                ChannelId = 11,
                Endpoint = API_ENDPOINT,
                MessageIdentification = header.MessageIdentification,
                CreatedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(-5)),
                StackTrace = tracer,
                Message = ex.Message
            });

            //return StatusCode(500, EasyResponseHelper.EasyInternalErrorRespond(12001));
            return StatusCode(500, tracer);
        }
    }
}
