using Domain.Entities.Config;
using Domain.Interfaces;
using MongoDB.Driver;
using MongodbInfrastructure.Collections;

namespace MongodbInfrastructure.Repositories;

public class MongoChannelConfigRepository : IChannelConfigRepository
{
    private readonly IMongoCollection<ChannelConfigDocument> _collection;

    public MongoChannelConfigRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ChannelConfigDocument>("MicroserviceConfig");
        EnsureIndexes();
    }

    public async Task<ChannelEntity?> GetByChannelIdAsync(int idCanal, CancellationToken ct = default)
    {
        var filter = Builders<ChannelConfigDocument>.Filter.Eq(x => x.IdCanal, idCanal);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    private void EnsureIndexes()
    {
        var indexes = new List<CreateIndexModel<ChannelConfigDocument>>
        {
            new(
                Builders<ChannelConfigDocument>.IndexKeys.Ascending(x => x.IdCanal),
                new CreateIndexOptions { Name = "ix_config_id_canal", Unique = true }
            )
        };

        _collection.Indexes.CreateMany(indexes);
    }
}
