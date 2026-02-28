using Application.Ports;
using Application.Usecases.ErrorTrace;
using Application.Usecases.Healthcheck;
using Application.Usecases.Signature.SignatureDocumentStatusQuery;
using Application.Usecases.Signature.SignatureStatusQuery;
using Application.Usecases.Signature.SignatureContractCreation;
using Application.Usecases.Signature.SignatureContractCancellation;
using Application.Usecases.Signature.ProviderDocumentsUpdate;
using Application.Usecases.Signature.SignedDocumentsUpdate;
using Domain.Containers.MemoryEvent;
using AwsSqsInfrastructure;
using InternalHttpClientInfrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongodbInfrastructure;

/* ********************************************************************************************************          
# * Copyright © 2026 Arify Labs - All rights reserved.   
# * 
# * Info                  : System API Template.
# *
# * By                    : Victor Jhampier Caxi Maquera
# * Email/Mobile/Phone    : victorjhampier@gmail.com | 968991*14
# *
# * Creation date         : 03/08/2026
# * 
# * Docs for json Ignore
# * https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/ignore-properties
# **********************************************************************************************************/

namespace Application;

public static class ApplicationSetting
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        // Added Queue in Memory for Api Events
        services.AddSingleton<MicroserviceCallMemoryQueue>(provider => new MicroserviceCallMemoryQueue());
        services.AddSingleton<MicroserviceErrorMemoryQueue>(provider => new MicroserviceErrorMemoryQueue());

        // Added Infrstrutures
        services.AddKeynuaInfrastructure(configuration, isDevelopment);
        services.AddMongodbInfrastructure(configuration, isDevelopment);
        services.AddAwsSqsInfrastructure(configuration, isDevelopment);

        //Dependency inyection        
        services.AddTransient<ISignatureContractPort, DocumentSignatureUsecase>();
        services.AddTransient<ICancelSignatureContractPort, DocumentSignatureCancellation>();
        services.AddTransient<IUpdateSignedDocumentsPort, SignedDocumentsUpdateCase>();
        services.AddTransient<IUpdateProviderDocumentsPort, DocumentSignatureCompletionUsecase>();
        services.AddTransient<IMicroserviceTracePort, MicroserviceTracePersistenceCase>();
        services.AddTransient<IGetSignatureStatusPort, DocumentSignatureInquiryUsecase>();
        services.AddTransient<IGetSignatureDocumentStatusPort, SignatureDocumentStatusQueryCase>();
        services.AddTransient<IHealthcheckPort, HealthcheckUsecase>();
    }
}
