# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV TZ=America/Lima

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SystemAPI/SystemAPI.csproj", "SystemAPI/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["EventListener/EventListener.csproj", "EventListener/"]
COPY ["AwsSqsInfrastructure/AwsSqsInfrastructure.csproj", "AwsSqsInfrastructure/"]
COPY ["FakeApiInfrastructure/FakeApiInfrastructure.csproj", "FakeApiInfrastructure/"]
COPY ["InternalHttpClientInfrastructure/KeynuaInfrastructure.csproj", "InternalHttpClientInfrastructure/"]
COPY ["MongodbInfrastructure/MongodbInfrastructure.csproj", "MongodbInfrastructure/"]

RUN dotnet restore "SystemAPI/SystemAPI.csproj"

COPY . .
WORKDIR "/src/SystemAPI"
RUN dotnet build "SystemAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SystemAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Certificado para DocumentDB
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget \
    && wget -O /etc/ssl/certs/global-bundle.pem https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "SystemAPI.dll"]

#------- To Deploy ----------------------------------------------
# docker build -t document-services-s .
# docker run --restart=on-failure -p 8080:8080 \
#   -e ASPNETCORE_URLS=http://+:8080 \
#   -e ASPNETCORE_ENVIRONMENT=Production \
#   -e JWT_ISSUER=... \
#   -e JWT_AUDIENCE=... \
#   -e JWT_READ_SCOPE=... \
#   -e JWT_UPDATE_SCOPE=... \
#   -e JWT_WRITE_SCOPE=... \
#   -e MONGO_DB_SERVER=... \
#   -e MONGO_DB_NAME=... \
#   -e MONGO_DB_USER=... \
#   -e MONGO_DB_PASSWD=... \
#   -e KEYNUA_BASE_URL=... \
#   -e KEYNUA_API_KEY=... \
#   -e KEYNUA_AUTHORIZATION=... \
#   -e KEYNUA_TEMPLATE_ID=... \
#   -e KEYNUA_BANKING=... \
#   -e KEYNUA_PRODUCT=... \
#   -e KEYNUA_EXPIRATION_IN_HOURS=... \
#   -e AWS_SQS_REGION=... \
#   -e AWS_SQS_SERVICE_URL=... \
#   -e AWS_SQS_PROFILE_NAME=... \
#   -e AWS_SQS_ACCESS_KEY=... \
#   -e AWS_SQS_SECRET_KEY=... \
#   -e AWS_SQS_QUEUE_URL=... \
#   -e AWS_SQS_MESSAGE_GROUP_ID=... \
#   -e AWS_SQS_MESSAGE_DEDUPLICATION_ID=... \
#   --name document-services-s -d document-services-s
