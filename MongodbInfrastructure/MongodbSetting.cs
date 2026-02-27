using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Domain.Commons;
using MongodbInfrastructure.Commnads;
using MongodbInfrastructure.Queries;
namespace MongodbInfrastructure;

public static class MongodbSetting
{
    public static void AddMongodbInfrastructure(this IServiceCollection services,IConfiguration configuration,bool isDevelopment)
    {
        var mongoConfig = ResolveMongoConfig(configuration, isDevelopment);

        services.AddSingleton(mongoConfig);

        services.AddSingleton<IMongoClient>(sp =>
            BuildMongoClient(sp.GetRequiredService<MongoConnectionConfig>()));

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var cfg = sp.GetRequiredService<MongoConnectionConfig>();
            return sp.GetRequiredService<IMongoClient>().GetDatabase(cfg.DatabaseName);
        });

        services.AddScoped<ISignatureQuery, MongoSignatureQuery>();
        services.AddScoped<ISignatureCommand, MongoSignatureCommand>();
        services.AddScoped<IChannelQuery, MongoChannelQuery>();
        services.AddScoped<IMicroserviceTraceRepository, MongoMicroserviceTraceCommand>();
    }

    private static IMongoClient BuildMongoClient(MongoConnectionConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(cfg.DatabaseName))
            throw new InvalidOperationException("MongoDB database name is not configured.");

        var scheme = "mongodb://";
        var server = cfg.Server;
        var user = Uri.EscapeDataString(cfg.User);
        var password = Uri.EscapeDataString(cfg.Password);

        var conn = $"{scheme}{user}:{password}@{server}/{cfg.DatabaseName}";

        var settings = MongoClientSettings.FromConnectionString(conn);
        settings.ApplicationName = "document-services-s";
        settings.RetryReads = true;
        settings.ReadPreference = ReadPreference.SecondaryPreferred;
        settings.WriteConcern = WriteConcern.WMajority;
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        settings.ConnectTimeout = TimeSpan.FromSeconds(15);
        settings.SocketTimeout = TimeSpan.FromSeconds(10);
        settings.ReadPreference = ReadPreference.Primary;
        settings.MaxConnectionPoolSize = 300;  // ejemplo, ajústalo
        settings.MinConnectionPoolSize = 20;
        settings.MaxConnecting = 10;           // limita conexiones “creándose” a la vez (útil bajo picos)

        return new MongoClient(settings);
    }

    private static MongoConnectionConfig ResolveMongoConfig(IConfiguration configuration, bool isDevelopment)
    {
        return isDevelopment
            ? ReadFromAppSettings(configuration)
            : ReadFromEnvironment();
    }

    private static MongoConnectionConfig ReadFromAppSettings(IConfiguration configuration)
    {
        var section = configuration.GetSection("MongoStoreDatabase");

        string GetCfg(string key) =>
            section[key] ?? throw new InvalidOperationException(
                $"La clave de configuración 'MongoStoreDatabase:{key}' no está definida.");

        return new MongoConnectionConfig(
            Server: GetCfg("MONGO_DB_SERVER"),
            DatabaseName: GetCfg("MONGO_DB_NAME"),
            User: CryptoCommon.decryptString(GetCfg("MONGO_DB_USER")),
            Password: CryptoCommon.decryptString(GetCfg("MONGO_DB_PASSWD")));
    }

    private static MongoConnectionConfig ReadFromEnvironment()
    {
        string GetEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException(
                $"La variable de entorno '{name}' no esta definida.");

        return new MongoConnectionConfig(
            Server: GetEnv("MONGO_DB_SERVER"),
            DatabaseName: GetEnv("MONGO_DB_NAME"),
            User: CryptoCommon.decryptString(GetEnv("MONGO_DB_USER")),
            Password: CryptoCommon.decryptString(GetEnv("MONGO_DB_PASSWD")));
    }

    private sealed record MongoConnectionConfig(
        string Server,
        string DatabaseName,
        string User,
        string Password);
}
