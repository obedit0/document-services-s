using Domain.Interfaces;
using Domain.Commons;
using InternalHttpClientInfrastructure.Collections;
using InternalHttpClientInfrastructure.Commands;
using InternalHttpClientInfrastructure.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http.Headers;

/* ********************************************************************************************************          
# * Copyright © 2026 Arify Labs - All rights reserved.   
# * 
# * Info                  : Http Conector Dynamic Keep-Alive.
# *
# * By                    : Victor Jhampier Caxi Maquera
# * Email/Mobile/Phone    : victorjhampier@gmail.com | 968991714
# *
# * Creation date         : 01/01/2026
# * 
# **********************************************************************************************************/

namespace InternalHttpClientInfrastructure;

public static class KeynuaSetting
{
    public static void AddKeynuaInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var keynuaOptions = BuildKeynuaOptions(configuration, isDevelopment);
        services.AddSingleton<IOptions<KeynuaContext>>(Options.Create(keynuaOptions));

        services.AddHttpClient("ArifyClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(25);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // === Equivalentes a tu pool ===
            MaxConnectionsPerServer = 200,   // max_connections
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30), // keepalive_expiry (idle)
            PooledConnectionLifetime = TimeSpan.FromMinutes(5), // rotación para DNS/TLS
            // Keep-alive HTTP/2 y HTTP/1.1 se gestiona automáticamente por el handler
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        });

        services.AddHttpClient("ArifyClient")
        .ConfigureHttpClient(client =>
        {
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
        });

        services.AddHttpClient("ArifyClient")
        .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(200)));
        services.AddScoped<ISignatureContractQuery, KeynuaContractQuery>();
        services.AddScoped<ISignatureContractCommand, KeynuaContractCommand>();
    }

    private static KeynuaContext BuildKeynuaOptions(IConfiguration configuration, bool isDevelopment)
    {
        var options = new KeynuaContext();

        if (isDevelopment)
        {
            options.BaseUrl = configuration["Keynua:BaseUrl"];
            options.ApiKey = CryptoCommon.decryptString(configuration["Keynua:ApiKey"]);
            options.Authorization = CryptoCommon.decryptString(configuration["Keynua:Authorization"]);
            options.Banking = configuration["Keynua:Banking"];
            options.Product = configuration["Keynua:Product"];

            return options;
        }

        string GetEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"La variable de entorno '{name}' no esta definida.");

        options.BaseUrl = GetEnv("KEYNUA_BASE_URL");
        options.ApiKey = CryptoCommon.decryptString(GetEnv("KEYNUA_AUTH_API_KEY"));
        options.Authorization = CryptoCommon.decryptString(GetEnv("KEYNUA_AUTH_AUTHORIZATION"));
        options.Banking = GetEnv("KEYNUA_BANKING");
        options.Product = GetEnv("KEYNUA_PRODUCT");

        return options;
    }
}
