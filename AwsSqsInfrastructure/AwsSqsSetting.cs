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
    public static void AddAwsSqsInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var awsOptions = BuildAwsSqsOptions(configuration, isDevelopment);
        services.AddSingleton(awsOptions);
        services.AddSingleton<IOptions<AwsSqsOptions>>(Options.Create(awsOptions));

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

    private static AwsSqsOptions BuildAwsSqsOptions(IConfiguration configuration, bool isDevelopment)
    {
        var options = new AwsSqsOptions();

        if (isDevelopment)
        {
            options.Region = configuration["AwsSqs:Region"];
            options.ServiceUrl = configuration["AwsSqs:ServiceUrl"];
            options.ProfileName = configuration["AwsSqs:ProfileName"];
            options.AccessKey = configuration["AwsSqs:AccessKey"];
            options.SecretKey = configuration["AwsSqs:SecretKey"];
            options.QueueUrl = configuration["AwsSqs:QueueUrl"];
            options.MessageGroupId = configuration["AwsSqs:MessageGroupId"];
            options.MessageDeduplicationId = configuration["AwsSqs:MessageDeduplicationId"];
            return options;
        }

        string GetEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"La variable de entorno '{name}' no esta definida.");

        options.Region = GetEnv("AWS_SQS_REGION");
        options.ServiceUrl = GetEnv("AWS_SQS_SERVICE_URL");
        options.ProfileName = GetEnv("AWS_SQS_PROFILE_NAME");
        options.AccessKey = GetEnv("AWS_SQS_ACCESS_KEY");
        options.SecretKey = GetEnv("AWS_SQS_SECRET_KEY");
        options.QueueUrl = GetEnv("AWS_SQS_QUEUE_URL");
        options.MessageGroupId = GetEnv("AWS_SQS_MESSAGE_GROUP_ID");
        options.MessageDeduplicationId = GetEnv("AWS_SQS_MESSAGE_DEDUPLICATION_ID");

        return options;
    }
}
