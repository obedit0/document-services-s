using Domain.Entities.Config;

namespace Domain.Interfaces;

public interface IChannelConfigRepository
{
    Task<ChannelEntity?> GetByChannelIdAsync(int idCanal, CancellationToken ct = default);
}
