using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http.Headers;

/* ********************************************************************************************************          
# * Copyright � 2026 Arify Labs - All rights reserved.   
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

public static class InternalHttpClientSetting
{
    public static void AddInternalHttpClientConnector(this IServiceCollection services)
    {
        services.AddHttpClient("ArifyClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(25);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // === Equivalentes a tu pool ===
            MaxConnectionsPerServer = 10,   // max_connections
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
    }
}