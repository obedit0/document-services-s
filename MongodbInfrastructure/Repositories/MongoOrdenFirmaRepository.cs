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

    public async Task<OrdenFirma?> GetByReferenciaAsync(string referencia, CancellationToken ct = default)
    {
        var filter = Builders<OrdenFirmaDocument>.Filter.Eq(x => x.Referencia, referencia);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task InsertAsync(OrdenFirma entity, CancellationToken ct = default)
    {
        var document = OrdenFirmaDocument.FromDomain(entity);
        await _collection.InsertOneAsync(document, cancellationToken: ct);
    }

    private void EnsureIndexes()
    {
        var indexes = new List<CreateIndexModel<OrdenFirmaDocument>>
        {
            new(
                Builders<OrdenFirmaDocument>.IndexKeys.Ascending(x => x.Referencia),
                new CreateIndexOptions { Unique = true, Name = "ux_orden_firma_referencia" }
            ),
            new(
                Builders<OrdenFirmaDocument>.IndexKeys
                    .Ascending(x => x.Proveedor)
                    .Ascending(x => x.IdOrdenProveedor),
                new CreateIndexOptions { Name = "ix_orden_firma_proveedor_orden" }
            ),
            new(
                Builders<OrdenFirmaDocument>.IndexKeys
                    .Ascending(x => x.Estado)
                    .Ascending(x => x.FechaCreacion),
                new CreateIndexOptions { Name = "ix_orden_firma_estado_fecha" }
            )
        };

        _collection.Indexes.CreateMany(indexes);
    }
}
