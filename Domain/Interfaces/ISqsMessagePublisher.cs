using Domain.Entities.Internals;

namespace Domain.Interfaces;

public interface ISqsMessagePublisher
{
    Task<SqsSendResult> SendAsync(string messageBody, CancellationToken ct = default);
    Task<bool> HealthcheckAsync(CancellationToken ct = default);
}
