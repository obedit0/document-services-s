using Domain.Entities;

namespace Domain.Interfaces;
public interface IExampleTitleQuery
{
    public Task<ExampleTitleEntity> GetAsync(int value = 1, CancellationToken ct = default);
    public Task<ExampleTitleEntity> GetProductAsync(int value = 1, CancellationToken ct = default);
}
