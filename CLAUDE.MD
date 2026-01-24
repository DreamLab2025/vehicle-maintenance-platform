# Verendar Project - Claude Context

## Project Overview
Verendar is a microservices-based application built with .NET 9 and .NET Aspire orchestration. The system consists of multiple domain-driven services following clean architecture principles.

## Architecture

### Technology Stack
- **.NET 9.0** - Target framework
- **.NET Aspire 9.5.0** - Cloud-native orchestration
- **PostgreSQL** - Primary database
- **RabbitMQ** - Message broker for async communication
- **Redis** - Caching and session management
- **YARP** - Reverse proxy/API gateway

### Solution Structure

```
Verendar/
├── Verendar.AppHost/              # Aspire orchestration host
├── Verendar.ServiceDefaults/      # Shared service configurations
├── Verendar.Common/               # Shared utilities and cross-cutting concerns
├── Verendar.DatabaseMigrationHelpers/ # Database migration utilities
├── Identity/                      # Identity & authentication service
│   ├── Verendar.Identity/        # API/Host project
│   └── Verender.Identity.Contracts/ # Shared contracts
├── Media/                         # Media management service
│   ├── Verendar.Media/           # API/Host project
│   ├── Verendar.Media.Application/ # Use cases & business logic
│   ├── Verendar.Media.Domain/    # Domain models & entities
│   └── Verendar.Media.Infrastructure/ # Data access & external services
├── Vehicle/                       # Vehicle management service
│   ├── Verendar.Vehicle/         # API/Host project
│   ├── Verendar.Vehicle.Application/
│   ├── Verendar.Vehicle.Domain/
│   └── Verendar.Vehicle.Infrastructure/
└── Notification/                  # Notification service
    ├── Verendar.Notification/    # API/Host project
    ├── Verendar.Notification.Application/
    ├── Verendar.Notification.Domain/
    └── Verendar.Notification.Infrastructure/
```

## Services

### 1. Identity Service
- **Purpose**: User authentication, authorization, and identity management
- **Architecture**: Monolithic service (single project)
- **Database**: UserDbContext with PostgreSQL
- **Key Features**:
  - User registration and authentication
  - Nullable email property support
  - User Secrets for API key management
  - OTP handling

### 2. Media Service
- **Purpose**: Media file management and processing
- **Architecture**: Clean Architecture (Domain/Application/Infrastructure layers)
- **Responsibilities**:
  - File upload and storage
  - Media metadata management
  - Asset serving

### 3. Vehicle Service
- **Purpose**: Vehicle information and management
- **Architecture**: Clean Architecture (Domain/Application/Infrastructure layers)
- **Responsibilities**:
  - Vehicle registration and tracking
  - Vehicle-related business operations

### 4. Notification Service
- **Purpose**: Multi-channel notification delivery
- **Architecture**: Clean Architecture (Domain/Application/Infrastructure layers)
- **Key Features**:
  - OTP generation and delivery
  - Multi-channel support (email, SMS, etc.)
  - Notification factory pattern
  - Event-driven architecture (subscribes to user registration events)

## Development Patterns

### Clean Architecture
Services following Clean Architecture (Media, Vehicle, Notification) are organized as:
- **Domain**: Core business entities and rules
- **Application**: Use cases, business logic, DTOs
- **Infrastructure**: Data access, external services, implementations
- **Host**: API endpoints, service configuration

### Service Communication
- **Synchronous**: HTTP/REST APIs (via YARP gateway)
- **Asynchronous**: Event-driven via RabbitMQ
- **Example**: User registration event triggers notification service

### Database Strategy
- Each service has its own database context
- Database migrations managed via `Verendar.DatabaseMigrationHelpers`
- Migration runs on application startup using `MigrateDbContextAsync<T>()`

## Build & Run

### Prerequisites
- .NET 9.0 SDK
- Docker (for PostgreSQL, RabbitMQ, Redis)
- Visual Studio 2022 17.5+ or Rider

### Running the Solution
```bash
# Start all services via Aspire
dotnet run --project Verendar.AppHost

# Build entire solution
dotnet build Verendar.sln

# Run specific service
dotnet run --project Identity/Verendar.Identity
```

### Configuration
- User Secrets enabled for sensitive data (API keys)
- appsettings.json for service configuration
- Aspire AppHost manages infrastructure (databases, message brokers)

## Recent Changes

Based on git history:
1. **User email nullability** - Email property made nullable in User entity
2. **Enhanced notification system** - OTP handling and channel factory implementation
3. **Security improvements** - Removed API keys from appsettings.json, enabled User Secrets
4. **User registration events** - Event-driven notification trigger on user registration

## Key Files

- `Verendar.AppHost/AppHost.cs` - Aspire orchestration entry point
- `Verendar.AppHost/Extensions/` - Service registration and configuration
- `*/Program.cs` - Individual service entry points
- `*/Bootstrapping/` - Service-specific dependency injection setup

## Coding Conventions

- Use C# nullable reference types
- Follow Clean Architecture for domain services
- Event-driven communication for cross-service operations
- Database migrations run automatically on startup
- User Secrets for sensitive configuration

## Working with This Codebase

### Adding a New Service
1. Create service folder with Domain/Application/Infrastructure/Host projects
2. Register in AppHost via extension methods
3. Add database context and migration support
4. Configure in Aspire orchestration

### Adding Features
1. Start in Domain layer (entities, value objects)
2. Implement use cases in Application layer
3. Add infrastructure implementations
4. Expose via API endpoints in Host project

### Database Changes
1. Modify entity in Domain layer
2. Update DbContext in Infrastructure
3. Migration runs automatically on next startup

## Notes for AI Assistants

- This is a greenfield .NET Aspire project following modern practices
- Services use clean architecture except Identity (simpler monolithic design)
- Focus on domain-driven design and separation of concerns
- Event-driven patterns used for cross-service communication
- Security-conscious: no secrets in source control
