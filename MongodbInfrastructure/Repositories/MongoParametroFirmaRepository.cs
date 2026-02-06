using Domain.Entities.SignatureContract;
using Domain.Interfaces;
using MongoDB.Driver;
using MongodbInfrastructure.Collections;

namespace MongodbInfrastructure.Repositories
{
    public class MongoParametroFirmaRepository : IParametroFirmaRepository
    {
        private readonly IMongoCollection<ParametroFirmaDocument> _collection;

        public MongoParametroFirmaRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<ParametroFirmaDocument>("parametros_firma_canal");
        }
        public async Task<ParametroFirma?> ObtenerConfiguracionAsync(int idCanal, string nombreParametro, CancellationToken ct = default)
        {
            var filter = Builders<ParametroFirmaDocument>.Filter.And(
            Builders<ParametroFirmaDocument>.Filter.Eq(x => x.IdCanal, idCanal),
            Builders<ParametroFirmaDocument>.Filter.Eq(x => x.NombreParametro, nombreParametro));

            var doc = await _collection.Find(filter).FirstOrDefaultAsync(ct);
            return doc?.ToDomain();
        }
    }
}