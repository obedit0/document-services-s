using Domain.Entities.Config;

namespace Domain.Interfaces;

public interface IChannelQuery
{
    Task<ChannelEntity?> GetByChannelIdAsync(int idCanal, CancellationToken ct = default);
}
