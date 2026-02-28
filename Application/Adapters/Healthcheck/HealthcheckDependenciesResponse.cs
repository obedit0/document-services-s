namespace Application.Adapters.Healthcheck;

public sealed class HealthcheckDependenciesResponse
{
    public bool MongoDb { get; init; }
    public bool Keynua { get; init; }
    public bool Sqs { get; init; }
    public bool Overall { get; init; }
    public DateTime CheckedAt { get; init; }
}
