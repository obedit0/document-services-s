using Amazon;
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

            if (!string.IsNullOrWhiteSpace(options.Region))
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
            }

            return new AmazonSQSClient(config);
        });

        services.AddScoped<ISqsMessagePublisher, SqsMessagePublisher>();

        services.AddHealthChecks()
            .AddCheck<AwsSqsHealthCheck>("aws-sqs", tags: new[] { "ready" });
    }

    private static AwsSqsOptions BuildAwsSqsOptions(IConfiguration configuration, bool isDevelopment)
    {
        var options = new AwsSqsOptions();

        if (isDevelopment)
        {
            options.Region = configuration["AwsSqs:Region"];
            options.QueueUrl = configuration["AwsSqs:QueueUrl"];
            options.MessageGroupId = configuration["AwsSqs:MessageGroupId"];
            return options;
        }

        string GetEnv(string name) =>
            Environment.GetEnvironmentVariable(name)
            ?? throw new InvalidOperationException($"La variable de entorno '{name}' no esta definida.");

        options.Region = GetEnv("AWS_SQS_REGION");
        options.QueueUrl = GetEnv("AWS_SQS_QUEUE_URL");
        options.MessageGroupId = GetEnv("AWS_SQS_MESSAGE_GROUP_ID");

        return options;
    }
}
