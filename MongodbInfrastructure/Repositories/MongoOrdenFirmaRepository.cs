using Domain.Entities.SignatureContracts;
using Domain.Interfaces;
using MongoDB.Driver;
using MongodbInfrastructure.Collections;

namespace MongodbInfrastructure.Repositories;

public class MongoOrdenFirmaRepository : IOrdenFirmaRepository
{
    private readonly IMongoCollection<OrdenFirmaDocument> _collection;

    public MongoOrdenFirmaRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<OrdenFirmaDocument>("orden_firma");
        EnsureIndexes();
    }

    public async Task<OrdenFirma?> GetByKeywordAsync(string keyword, CancellationToken ct = default)
    {
        var filter = Builders<OrdenFirmaDocument>.Filter.Eq(x => x.Id,keyword);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<OrdenFirma?> GetByProviderIdAsync(string idOrdenProveedor, CancellationToken ct = default)
    {
        var filter = Builders<OrdenFirmaDocument>.Filter.Eq(x => x.IdOrdenProveedor, idOrdenProveedor);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<string> InsertAsync(OrdenFirma entity, CancellationToken ct = default)
    {
        var document = OrdenFirmaDocument.FromDomain(entity);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
        return document.Id;
    }

    public async Task<bool> UpdateAsync(OrdenFirma entity, CancellationToken ct = default)
    {
        var document = OrdenFirmaDocument.FromDomain(entity);
        var filter = Builders<OrdenFirmaDocument>.Filter.Eq(x => x.Keyword, entity.Keyword);
        
        var update = Builders<OrdenFirmaDocument>.Update
            .Set(x => x.Documentos, document.Documentos)
            .Set(x => x.Estado, document.Estado)
            .Set(x => x.FechaActualizacion, document.FechaActualizacion)
            .Set(x => x.Historico, document.Historico);

        var result = await _collection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    private void EnsureIndexes()
    {
        var indexes = new List<CreateIndexModel<OrdenFirmaDocument>>
        {
            new(
                Builders<OrdenFirmaDocument>.IndexKeys.Ascending(x => x.Keyword),
                new CreateIndexOptions { Name = "ix_orden_firma_keyword" }
            ),
            new(
                Builders<OrdenFirmaDocument>.IndexKeys.Ascending(x => x.IdOrdenProveedor),
                new CreateIndexOptions { Name = "ix_orden_firma_id_orden_proveedor" }
            )
        };

        _collection.Indexes.CreateMany(indexes);
    }
}
