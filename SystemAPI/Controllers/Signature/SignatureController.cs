using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Adapters.UpdateDocuments;
using Application.Ports;
using Microsoft.AspNetCore.Mvc;
using SystemAPI.Attributes;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.Signature;

[Route("document-services-s/signature-request")]
[ApiController]
public class SignatureController : ControllerBase
{
    private readonly ISignatureContractPort _signaturePort;
    private readonly IUpdateSignedDocumentsPort _updateSignedDocumentsPort;
    private readonly IUpdateProviderDocumentsPort _updateProviderDocumentsPort;
    private readonly IGetOrderByProviderIdPort _getOrderByProviderIdPort;
    private readonly IGetSignatureStatusPort _getSignatureStatusPort;
    private readonly ILogger<SignatureController> _logger;

    public SignatureController(
        ILogger<SignatureController> logger,
        ISignatureContractPort signaturePort,
        IUpdateSignedDocumentsPort updateSignedDocumentsPort,
        IUpdateProviderDocumentsPort updateProviderDocumentsPort,
        IGetOrderByProviderIdPort getOrderByProviderIdPort,
        IGetSignatureStatusPort getSignatureStatusPort)
    {
        _logger = logger;
        _signaturePort = signaturePort;
        _updateSignedDocumentsPort = updateSignedDocumentsPort;
        _updateProviderDocumentsPort = updateProviderDocumentsPort;
        _getOrderByProviderIdPort = getOrderByProviderIdPort;
        _getSignatureStatusPort = getSignatureStatusPort;
    }

    #region GET
    [HttpGet("retrieve")]
    [DefaultErrorCode("12004")]
    public async Task<IActionResult> GetByProviderId(
        [FromHeader] SignatureHeaderRequest header,
        [FromQuery] string idOrdenProveedor)
    {
        var request = new GetOrderByProviderIdRequest { IdOrdenProveedor = idOrdenProveedor };
        var result = await _getOrderByProviderIdPort.ExecuteAsync(header, request);

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

    [HttpGet("document-services-s/signature/status/retrieve")]
    [DefaultErrorCode("12005")]
    public async Task<IActionResult> RetrieveStatus(
        [FromHeader] SignatureHeaderRequest header,
        [FromQuery] GetSignatureStatusRequest query)
    {
        var result = await _getSignatureStatusPort.ExecuteAsync(header, query);

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
    #endregion

    #region POST
    [HttpPost("initiate")]
    [DefaultErrorCode("12001")]
    public async Task<IActionResult> Create(
        SignatureHeaderRequest header,
        [FromBody] CreateSignatureContractRequest body)
    {
        var result = await _signaturePort.CreateAsync(header, body);

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
    #endregion

    #region PUT
    [HttpPut("system-signature-s/v1/contracts/signed-documents")]
    [DefaultErrorCode("12002")]
    public async Task<IActionResult> UpdateSignedDocuments(
        [FromHeader] SignatureHeaderRequest header,
        [FromBody] UpdateSignedDocumentsRequest body)
    {
        var result = await _updateSignedDocumentsPort.ExecuteAsync(header, body);

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

    [HttpPut("system-signature-s/v1/contracts/provider-documents")]
    [DefaultErrorCode("12003")]
    public async Task<IActionResult> UpdateProviderDocuments(
        [FromHeader] SignatureHeaderRequest header,
        [FromBody] UpdateProviderDocumentsRequest body)
    {
        var result = await _updateProviderDocumentsPort.ExecuteAsync(header, body);

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
    #endregion
}
