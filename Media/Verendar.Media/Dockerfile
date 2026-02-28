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
COPY ["Media/Verendar.Media/Verendar.Media.csproj", "Media/Verendar.Media/"]
COPY ["App/Verendar.ServiceDefaults/Verendar.ServiceDefaults.csproj", "App/Verendar.ServiceDefaults/"]
COPY ["App/Verendar.DatabaseMigrationHelpers/Verendar.DatabaseMigrationHelpers.csproj", "App/Verendar.DatabaseMigrationHelpers/"]
COPY ["Media/Verendar.Media.Application/Verendar.Media.Application.csproj", "Media/Verendar.Media.Application/"]
COPY ["Media/Verendar.Media.Domain/Verendar.Media.Domain.csproj", "Media/Verendar.Media.Domain/"]
COPY ["App/Verendar.Common/Verendar.Common.csproj", "App/Verendar.Common/"]
COPY ["Media/Verendar.Media.Infrastructure/Verendar.Media.Infrastructure.csproj", "Media/Verendar.Media.Infrastructure/"]
RUN dotnet restore "./Media/Verendar.Media/Verendar.Media.csproj"
COPY . .
WORKDIR "/src/Media/Verendar.Media"
RUN dotnet build "./Verendar.Media.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Verendar.Media.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verendar.Media.dll"]