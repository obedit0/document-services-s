using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongodbInfrastructure.Repositories;

namespace MongodbInfrastructure;

public static class MongodbSetting
{
    public static void AddMongodbInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongodbOptions>(configuration.GetSection("ConnectionString"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            string conn = "mongodb://arodriguezf:Dev12345@10.5.81.16:27017/";
            
            var settings = MongoClientSettings.FromConnectionString(conn);
            settings.ApplicationName = "app-customer-credit-rating-s";
            settings.RetryReads = true;
            settings.ReadPreference = ReadPreference.SecondaryPreferred;
            settings.WriteConcern = WriteConcern.WMajority;
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.ConnectTimeout = TimeSpan.FromSeconds(5);
            settings.SocketTimeout = TimeSpan.FromSeconds(5);

            return new MongoClient(settings);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongodbOptions>>().Value;
            return sp.GetRequiredService<IMongoClient>().GetDatabase("DB_PruebasObesito");
        });

        services.AddScoped<IOrdenFirmaRepository, MongoOrdenFirmaRepository>();
        services.AddScoped<IChannelConfigRepository, MongoChannelConfigRepository>();
    }
}
