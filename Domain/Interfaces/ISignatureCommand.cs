using Domain.Entities.SignatureContracts;
using Domain.Enums;

namespace Domain.Interfaces;

public interface ISignatureCommand
{
    Task<string> InsertAsync(SignatureEntity entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(SignatureEntity entity, CancellationToken ct = default);
    Task UpdateStatusAsync(string id, SignatureStatus newStatus, HistoryEntity newHistory, CancellationToken ct = default);
    Task CancellationAsync(string id, SignatureStatus newStatus, HistoryEntity newHistory, CancellationToken ct = default);
    Task<bool> UpdateDocumentsAsync(string signatureId, List<DocumentEntity> documents, SignatureStatus status, HistoryEntity newHistory, CancellationToken ct = default);
}
