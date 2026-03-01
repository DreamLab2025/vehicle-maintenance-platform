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
COPY ["Notification/Verendar.Notification/Verendar.Notification.csproj", "Notification/Verendar.Notification/"]
COPY ["App/Verendar.Common/Verendar.Common.csproj", "App/Verendar.Common/"]
COPY ["App/Verendar.ServiceDefaults/Verendar.ServiceDefaults.csproj", "App/Verendar.ServiceDefaults/"]
COPY ["App/Verendar.DatabaseMigrationHelpers/Verendar.DatabaseMigrationHelpers.csproj", "App/Verendar.DatabaseMigrationHelpers/"]
COPY ["Notification/Verendar.Notification.Application/Verendar.Notification.Application.csproj", "Notification/Verendar.Notification.Application/"]
COPY ["Identity/Verender.Identity.Contracts/Verender.Identity.Contracts.csproj", "Identity/Verender.Identity.Contracts/"]
COPY ["Vehicle/Verendar.Vehicle.Contracts/Verendar.Vehicle.Contracts.csproj", "Vehicle/Verendar.Vehicle.Contracts/"]
COPY ["Notification/Verendar.Notification.Domain/Verendar.Notification.Domain.csproj", "Notification/Verendar.Notification.Domain/"]
COPY ["Notification/Verendar.Notification.Infrastructure/Verendar.Notification.Infrastructure.csproj", "Notification/Verendar.Notification.Infrastructure/"]
RUN dotnet restore "./Notification/Verendar.Notification/Verendar.Notification.csproj"
COPY . .
WORKDIR "/src/Notification/Verendar.Notification"
RUN dotnet build "./Verendar.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Verendar.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verendar.Notification.dll"]