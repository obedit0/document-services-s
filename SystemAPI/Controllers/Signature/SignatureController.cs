using Application.Adapters.Common;
using Application.Adapters.SignatureContracts;
using Application.Adapters.SignatureContracts.DocumentSignatureCompletion;
using Application.Adapters.SignatureContracts.UpdateDocuments;
using Application.Ports;
using Microsoft.AspNetCore.Mvc;
using SystemAPI.Attributes;
using SystemAPI.Helpers;

namespace SystemAPI.Controllers.Signature;

[Route("document-services-s/v1/signature-request")]
[ApiController]
public class SignatureController : ControllerBase
{
    private readonly ISignatureContractPort _signaturePort;
    private readonly ICancelSignatureContractPort _cancelSignatureContractPort;
    private readonly IUpdateSignedDocumentsPort _updateSignedDocumentsPort;
    private readonly IUpdateProviderDocumentsPort _updateProviderDocumentsPort;
    private readonly IGetSignatureStatusPort _getSignatureStatusPort;
    private readonly IGetSignatureDocumentStatusPort _getSignatureDocumentStatusPort;
    private readonly ILogger<SignatureController> _logger;

    public SignatureController(
        ILogger<SignatureController> logger,
        ISignatureContractPort signaturePort,
        ICancelSignatureContractPort cancelSignatureContractPort,
        IUpdateSignedDocumentsPort updateSignedDocumentsPort,
        IUpdateProviderDocumentsPort updateProviderDocumentsPort,
        IGetSignatureStatusPort getSignatureStatusPort,
        IGetSignatureDocumentStatusPort getSignatureDocumentStatusPort)
    {
        _logger = logger;
        _signaturePort = signaturePort;
        _cancelSignatureContractPort = cancelSignatureContractPort;
        _updateSignedDocumentsPort = updateSignedDocumentsPort;
        _updateProviderDocumentsPort = updateProviderDocumentsPort;
        _getSignatureStatusPort = getSignatureStatusPort;
        _getSignatureDocumentStatusPort = getSignatureDocumentStatusPort;
    }

    #region GET
    [HttpGet("consultar")]
    public async Task<IActionResult> RetrieveStatus(
        SignatureHeaderRequest header,
        [FromQuery] SignatureInquiryRequest query)
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
    [HttpPost("crear")]
    public async Task<IActionResult> Create(
        SignatureHeaderRequest header,
        [FromBody] SignatureRequest body)
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
    [HttpPut("guardar-documentos")]
    public async Task<IActionResult> UpdateSignedDocuments(
        SignatureHeaderRequest header,
        [FromBody] SignatureUpdateDocumentRequest body)
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

    [HttpPut("finalizar")]
    public async Task<IActionResult> UpdateProviderDocuments(
        SignatureHeaderRequest header,
        [FromBody] SignatureCompletionRequest body)
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

    [HttpPut("cancelar")]
    public async Task<IActionResult> CancelarOrden(
        SignatureHeaderRequest header,
        [FromQuery] int? keyword)
    {
        var request = new SignatureCancellationRequest
        {
            Keyword = keyword
        };

        var result = await _cancelSignatureContractPort.ExecuteAsync(header, request);

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

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocumentStatus(
        SignatureHeaderRequest header,
        [FromQuery] int? keyword)
    {
        var request = new GetSignatureDocumentStatusRequest
        {
            Keyword = keyword
        };

        var result = await _getSignatureDocumentStatusPort.ExecuteAsync(header, request);

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
