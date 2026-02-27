using Domain.Enums;

namespace InternalHttpClientInfrastructure.Mappers;

public static class KeynuaStatusMapper
{
    public static SignatureStatus MapSignatureStatus(string status)
    {
        return status switch
        {
            "pending_input" => SignatureStatus.PENDIENTE,
            "pending" => SignatureStatus.PENDIENTE,
            "working" => SignatureStatus.EN_PROCESO,
            "pending_approval" => SignatureStatus.EN_PROCESO,
            "contract_approval" => SignatureStatus.EN_PROCESO,
            "in_progress" => SignatureStatus.EN_PROCESO,
            "done" => SignatureStatus.COMPLETADO,
            "deleted" => SignatureStatus.ANULADO,
            "canceled" => SignatureStatus.ANULADO,
            "cancelled" => SignatureStatus.ANULADO,
            "expired" => SignatureStatus.EXPIRADO,
            "error" => SignatureStatus.ERROR,
            _ => SignatureStatus.ERROR
        };
    }
}
