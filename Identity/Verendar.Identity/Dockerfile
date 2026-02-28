# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Identity/Verendar.Identity/Verendar.Identity.csproj", "Identity/Verendar.Identity/"]
COPY ["App/Verendar.Common/Verendar.Common.csproj", "App/Verendar.Common/"]
COPY ["App/Verendar.ServiceDefaults/Verendar.ServiceDefaults.csproj", "App/Verendar.ServiceDefaults/"]
COPY ["App/Verendar.DatabaseMigrationHelpers/Verendar.DatabaseMigrationHelpers.csproj", "App/Verendar.DatabaseMigrationHelpers/"]
COPY ["Identity/Verendar.Identity.Application/Verendar.Identity.Application.csproj", "Identity/Verendar.Identity.Application/"]
COPY ["Identity/Verender.Identity.Contracts/Verender.Identity.Contracts.csproj", "Identity/Verender.Identity.Contracts/"]
COPY ["Identity/Verendar.Identity.Domain/Verendar.Identity.Domain.csproj", "Identity/Verendar.Identity.Domain/"]
COPY ["Identity/Verendar.Identity.Infrastructure/Verendar.Identity.Infrastructure.csproj", "Identity/Verendar.Identity.Infrastructure/"]
RUN dotnet restore "./Identity/Verendar.Identity/Verendar.Identity.csproj"
COPY . .
WORKDIR "/src/Identity/Verendar.Identity"
RUN dotnet build "./Verendar.Identity.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Verendar.Identity.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verendar.Identity.dll"]