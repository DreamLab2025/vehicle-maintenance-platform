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
COPY ["Ai/Verendar.Ai/Verendar.Ai.csproj", "Ai/Verendar.Ai/"]
COPY ["Ai/Verendar.Ai.Application/Verendar.Ai.Application.csproj", "Ai/Verendar.Ai.Application/"]
COPY ["Ai/Verendar.Ai.Domain/Verendar.Ai.Domain.csproj", "Ai/Verendar.Ai.Domain/"]
COPY ["App/Verendar.Common/Verendar.Common.csproj", "App/Verendar.Common/"]
COPY ["App/Verendar.ServiceDefaults/Verendar.ServiceDefaults.csproj", "App/Verendar.ServiceDefaults/"]
COPY ["App/Verendar.DatabaseMigrationHelpers/Verendar.DatabaseMigrationHelpers.csproj", "App/Verendar.DatabaseMigrationHelpers/"]
COPY ["Ai/Verendar.Ai.Infrastructure/Verendar.Ai.Infrastructure.csproj", "Ai/Verendar.Ai.Infrastructure/"]
RUN dotnet restore "./Ai/Verendar.Ai/Verendar.Ai.csproj"
COPY . .
WORKDIR "/src/Ai/Verendar.Ai"
RUN dotnet build "./Verendar.Ai.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Verendar.Ai.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verendar.Ai.dll"]