FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Location/Verendar.Location/Verendar.Location.csproj", "Location/Verendar.Location/"]
COPY ["App/Verendar.Common/Verendar.Common.csproj", "App/Verendar.Common/"]
COPY ["App/Verendar.ServiceDefaults/Verendar.ServiceDefaults.csproj", "App/Verendar.ServiceDefaults/"]
COPY ["App/Verendar.DatabaseMigrationHelpers/Verendar.DatabaseMigrationHelpers.csproj", "App/Verendar.DatabaseMigrationHelpers/"]
COPY ["Location/Verendar.Location.Application/Verendar.Location.Application.csproj", "Location/Verendar.Location.Application/"]
COPY ["Location/Verendar.Location.Domain/Verendar.Location.Domain.csproj", "Location/Verendar.Location.Domain/"]
COPY ["Location/Verendar.Location.Infrastructure/Verendar.Location.Infrastructure.csproj", "Location/Verendar.Location.Infrastructure/"]
RUN dotnet restore "./Location/Verendar.Location/Verendar.Location.csproj"
COPY . .
WORKDIR "/src/Location/Verendar.Location"
RUN dotnet build "./Verendar.Location.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Verendar.Location.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Verendar.Location.dll"]
