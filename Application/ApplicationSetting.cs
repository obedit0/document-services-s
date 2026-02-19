using Application.Ports;
using Application.Usecases.ErrorUsecase;
using Application.Usecases.ExampleUsecase;
using Application.Usecases.GetOrderByProviderIdUsecase;
using Application.Usecases.SignatureStatusUsecase;
using Application.Usecases.MicroserviceCallTraceUsecase;
using Application.Usecases.SignatureContractUsecase;
using Application.Usecases.SqsUsecase;
using Application.Usecases.UpdateProviderDocumentsUsecase;
using Application.Usecases.UpdateSignedDocumentsUsecase;
using Domain.Containers.MemoryEvent;
using AwsSqsInfrastructure;
using Domain.Interfaces;
using FakeApiInfrastructure;
using InternalHttpClientInfrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongodbInfrastructure;
using MongodbInfrastructure.Repositories;

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
        services.AddFakeApiInfrastructure();
        services.AddKeynuaInfrastructure(configuration,isDevelopment);
        services.AddMongodbInfrastructure(configuration);
        services.AddAwsSqsInfrastructure(configuration);

        //Dependency inyection        
        services.AddTransient<IExamplePort, ExampleCase>();
        services.AddTransient<ISignatureContractPort, SignatureContractCase>();
        services.AddTransient<IUpdateSignedDocumentsPort, UpdateSignedDocumentsCase>();
        services.AddTransient<IUpdateProviderDocumentsPort, UpdateProviderDocumentsCase>();
        services.AddTransient<IGetOrderByProviderIdPort, GetOrderByProviderIdCase>();
        services.AddTransient<IErrorInternalPort, ErrorInternalCase>();
        services.AddTransient<IGetSignatureStatusPort, GetSignatureStatusCase>();
        services.AddTransient<IMicroserviceCallTracePort, MicroserviceCallTraceSqsCase>();
        services.AddTransient<ISqsTestPort, SqsSendTestCase>();
        
        services.AddSingleton<IParametroFirmaRepository, MongoParametroFirmaRepository>();
    }
}
