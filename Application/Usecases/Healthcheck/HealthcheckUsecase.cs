using Application.Adapters.Healthcheck;
using Application.Internals.Executors;
using Application.Ports;
using Domain.Interfaces;

namespace Application.Usecases.Healthcheck;

public sealed class HealthcheckUsecase : IHealthcheckPort
{
    private readonly ISignatureQuery _signatureQuery;
    private readonly ISignatureContractQuery _signatureContractQuery;
    private readonly ISqsMessagePublisher _sqsMessagePublisher;

    public HealthcheckUsecase(
        ISignatureQuery signatureQuery,
        ISignatureContractQuery signatureContractQuery,
        ISqsMessagePublisher sqsMessagePublisher)
    {
        _signatureQuery = signatureQuery;
        _signatureContractQuery = signatureContractQuery;
        _sqsMessagePublisher = sqsMessagePublisher;
    }

    public async Task<EasyResult<HealthcheckDependenciesResponse>> ExecuteAsync(CancellationToken ct = default)
    {
        var mongoTask = SafeCheckAsync(() => _signatureQuery.HealthcheckAsync(ct));
        var keynuaTask = SafeCheckAsync(() => _signatureContractQuery.HealthcheckAsync(ct));
        var sqsTask = SafeCheckAsync(() => _sqsMessagePublisher.HealthcheckAsync(ct));

        await Task.WhenAll(mongoTask, keynuaTask, sqsTask);

        var response = new HealthcheckDependenciesResponse
        {
            MongoDb = mongoTask.Result,
            Keynua = keynuaTask.Result,
            Sqs = sqsTask.Result,
            Overall = mongoTask.Result && keynuaTask.Result && sqsTask.Result,
            CheckedAt = DateTime.UtcNow.AddHours(-5)
        };

        return EasyResult<HealthcheckDependenciesResponse>.Success(response);
    }

    private static async Task<bool> SafeCheckAsync(Func<Task<bool>> action)
    {
        try
        {
            return await action();
        }
        catch
        {
            return false;
        }
    }
}
