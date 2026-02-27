using Domain.Entities.SignatureContracts;
using Domain.Interfaces;
using MongoDB.Driver;
using MongodbInfrastructure.Collections;

namespace MongodbInfrastructure.Queries;

public class MongoSignatureQuery : ISignatureQuery
{
    private readonly IMongoCollection<SignatureDocument> _collection;

    public MongoSignatureQuery(IMongoDatabase database)
    {
        _collection = database.GetCollection<SignatureDocument>("signatures");
    }

    public async Task<SignatureEntity?> GetByKeywordAndChannelAsync(int keyword, int channel, CancellationToken ct = default)
    {
        var filter = Builders<SignatureDocument>.Filter.And(
            Builders<SignatureDocument>.Filter.Eq(x => x.Keyword, keyword),
            Builders<SignatureDocument>.Filter.Eq(x => x.Channel, channel),
            Builders<SignatureDocument>.Filter.Eq(x => x.Active, true)
        );
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<int> CountByKeywordAndChannelAsync(int keyword, int channel, CancellationToken ct = default)
    {
        var filter = Builders<SignatureDocument>.Filter.And(
            Builders<SignatureDocument>.Filter.Eq(x => x.Keyword, keyword),
            Builders<SignatureDocument>.Filter.Eq(x => x.Channel, channel)
        );
        var count = await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
        return (int)count;
    }
}
