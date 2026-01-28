namespace MongodbInfrastructure;

public sealed class MongodbOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
}
