using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SQS;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsSqsInfrastructure;

public static class AwsSqsSetting
{
    public static void AddAwsSqsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwsSqsOptions>(configuration.GetSection("AwsSqs"));

        services.AddSingleton<IAmazonSQS>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsSqsOptions>>().Value;
            var config = new AmazonSQSConfig();

            if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
            {
                config.ServiceURL = options.ServiceUrl;
                config.UseHttp = options.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrWhiteSpace(options.Region))
                {
                    config.AuthenticationRegion = options.Region;
                }
            }

            if (!string.IsNullOrWhiteSpace(options.Region))
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            if (!string.IsNullOrWhiteSpace(options.ProfileName))
            {
                var profileChain = new CredentialProfileStoreChain();
                if (!profileChain.TryGetAWSCredentials(options.ProfileName, out var credentials))
                {
                    throw new InvalidOperationException(
                        $"AWS profile '{options.ProfileName}' not found. Check ~/.aws/credentials or ~/.aws/config.");
                }

                return new AmazonSQSClient(credentials, config);
            }

            if (!string.IsNullOrWhiteSpace(options.AccessKey) &&
                !string.IsNullOrWhiteSpace(options.SecretKey))
            {
                return new AmazonSQSClient(options.AccessKey, options.SecretKey, config);
            }

            return new AmazonSQSClient(config);
        });

        services.AddScoped<ISqsMessagePublisher, SqsMessagePublisher>();
        services.AddScoped<IMicroserviceCallTracePublisher, SqsMicroserviceCallTracePublisher>();

        services.AddHealthChecks()
            .AddCheck<AwsSqsHealthCheck>("aws-sqs", tags: new[] { "ready" });
    }
}
