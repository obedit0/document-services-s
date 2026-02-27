namespace Application.Adapters.SignatureContracts;

public class SignatureInquiryResponse
{
    public int Keyword { get; set; }
    public string? IdOrdenKeynua { get; set; }
    public string? CEstadoGeneral { get; set; }
    public string? CEstadoCanal { get; set; }
    public List<SignatureStatusSignerResponse> ArrFirmantes { get; set; } = [];
}

public class SignatureStatusSignerResponse
{
    public string? CDNI { get; set; }
    public string? CNombreCompleto { get; set; }
    public string? CCorreo { get; set; }
    public string? CCelular { get; set; }
    public int NEstado { get; set; }
}
