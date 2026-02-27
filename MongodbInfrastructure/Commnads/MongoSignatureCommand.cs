using Domain.Entities.SignatureContracts;
using Domain.Enums;
using Domain.Interfaces;
using MongoDB.Driver;
using MongodbInfrastructure.Collections;

namespace MongodbInfrastructure.Commnads;

public class MongoSignatureCommand : ISignatureCommand
{
    private readonly IMongoCollection<SignatureDocument> _collection;

    public MongoSignatureCommand(IMongoDatabase database)
    {
        _collection = database.GetCollection<SignatureDocument>("signatures");
        EnsureIndexes();
    }

    public async Task<string> InsertAsync(SignatureEntity entity, CancellationToken ct = default)
    {
        var document = SignatureDocument.FromDomain(entity);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
        return document.Id;
    }

    public async Task<bool> UpdateAsync(SignatureEntity entity, CancellationToken ct = default)
    {
        var document = SignatureDocument.FromDomain(entity);
        var filter = Builders<SignatureDocument>.Filter.Eq(x => x.Id, entity.SignatureId!);

        var update = Builders<SignatureDocument>.Update
            .Set(x => x.Documents, document.Documents)
            .Set(x => x.Status, document.Status)
            .Set(x => x.UpdatedAt, document.UpdatedAt)
            .Set(x => x.History, document.History);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task UpdateStatusAsync(string id, SignatureStatus newStatus, HistoryEntity newHistory, CancellationToken ct = default)
    {
        var filter = Builders<SignatureDocument>.Filter.Eq(x => x.Id, id);

        var historyDocument = MapHistoryToDocument(newHistory);

        var update = Builders<SignatureDocument>.Update
            .Set(x => x.Status, newStatus.ToString())
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Push(x => x.History, historyDocument);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task CancellationAsync(string id, SignatureStatus newStatus, HistoryEntity newHistory, CancellationToken ct = default)
    {
        var filter = Builders<SignatureDocument>.Filter.Eq(x => x.Id, id);

        var historyDocument = MapHistoryToDocument(newHistory);

        var update = Builders<SignatureDocument>.Update
            .Set(x => x.Status, newStatus.ToString())
            .Set(x => x.UpdatedAt, DateTime.UtcNow)
            .Set(x => x.Active, false)
            .Push(x => x.History, historyDocument);

        await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
    }

    public async Task<bool> UpdateDocumentsAsync(
        string signatureId,
        List<DocumentEntity> documents,
        SignatureStatus status,
        HistoryEntity newHistory,
        CancellationToken ct = default)
    {
        var filter = Builders<SignatureDocument>.Filter.Eq(x => x.Id, signatureId);

        var documentsDoc = documents.Select(MapDocumentToDocument).ToList();
        var historyDoc = MapHistoryToDocument(newHistory);

        var update = Builders<SignatureDocument>.Update
            .Set(x => x.Documents, documentsDoc)
            .Set(x => x.Status, status.ToString())
            .Set(x => x.UpdatedAt, DateTime.UtcNow.AddHours(-5))
            .Push(x => x.History, historyDoc);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    private static DocumentDocument MapDocumentToDocument(DocumentEntity d) => new()
    {
        Name = d.Name,
        OwnerClients = d.OwnerClients ?? [],
        S3KeyOriginal = d.S3KeyOriginal,
        S3KeySigned = d.S3KeyFirmado,
        S3KeySignedExpiresAt = d.S3KeyFirmadoExpiresAt,
        ProviderKeySigned = d.ProviderKeyFirmado,
        ProviderKeySignedExpiresAt = d.ProviderKeyFirmadoExpiresAt,
        SignedAt = d.FechaFirma
    };

    private static HistoryEventDocument MapHistoryToDocument(HistoryEntity h) => new()
    {
        EventDate = h.EventDate,
        Source = h.Source,
        PreviousStatus = h.PreviousStatus?.ToString(),
        NewStatus = h.NewStatus.ToString(),
        Reason = h.Reason,
        ActorId = h.ActorId,
        ProviderEventId = h.ProviderEventId
    };

    private void EnsureIndexes()
    {
        var indexes = new List<CreateIndexModel<SignatureDocument>>
        {
            new(
                Builders<SignatureDocument>.IndexKeys.Ascending(x => x.Keyword),
                new CreateIndexOptions { Name = "ix_signature_keyword" }
            ),
            new(
                Builders<SignatureDocument>.IndexKeys.Ascending(x => x.ProviderIdentity),
                new CreateIndexOptions { Name = "ix_signature_provider_identity" }
            )
        };

        _collection.Indexes.CreateMany(indexes);
    }
}
