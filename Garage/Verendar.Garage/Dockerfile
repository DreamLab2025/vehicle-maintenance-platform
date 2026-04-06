FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Garage/Verendar.Garage/Verendar.Garage.csproj", "Garage/Verendar.Garage/"]
COPY ["App/Verendar.Common/Verendar.Common.csproj", "App/Verendar.Common/"]
COPY ["App/Verendar.ServiceDefaults/Verendar.ServiceDefaults.csproj", "App/Verendar.ServiceDefaults/"]
COPY ["App/Verendar.DatabaseMigrationHelpers/Verendar.DatabaseMigrationHelpers.csproj", "App/Verendar.DatabaseMigrationHelpers/"]
COPY ["Garage/Verendar.Garage.Contracts/Verendar.Garage.Contracts.csproj", "Garage/Verendar.Garage.Contracts/"]
COPY ["Garage/Verendar.Garage.Application/Verendar.Garage.Application.csproj", "Garage/Verendar.Garage.Application/"]
COPY ["Garage/Verendar.Garage.Domain/Verendar.Garage.Domain.csproj", "Garage/Verendar.Garage.Domain/"]
COPY ["Garage/Verendar.Garage.Infrastructure/Verendar.Garage.Infrastructure.csproj", "Garage/Verendar.Garage.Infrastructure/"]
COPY ["Payment/Verendar.Payment.Contracts/Verendar.Payment.Contracts.csproj", "Payment/Verendar.Payment.Contracts/"]
COPY ["Vehicle/Verendar.Vehicle.Contracts/Verendar.Vehicle.Contracts.csproj", "Vehicle/Verendar.Vehicle.Contracts/"]
RUN dotnet restore "./Garage/Verendar.Garage/Verendar.Garage.csproj"
COPY . .
WORKDIR "/src/Garage/Verendar.Garage"
RUN dotnet build "./Verendar.Garage.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Verendar.Garage.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verendar.Garage.dll"]
