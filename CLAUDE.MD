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
  - Vehicle maintenance scheduling and tracking
  - Default maintenance schedules by manufacturer
  - Part category management (engine oil, filters, etc.)

### 4. Notification Service
- **Purpose**: Multi-channel notification delivery
- **Architecture**: Clean Architecture (Domain/Application/Infrastructure layers)
- **Key Features**:
  - OTP generation and delivery
  - Multi-channel support (email, SMS, etc.)
  - Notification factory pattern
  - Event-driven architecture (subscribes to user registration events)

### 5. AI Service
- **Purpose**: AI-powered vehicle maintenance analysis
- **Architecture**: Clean Architecture (Domain/Application/Infrastructure layers)
- **Key Features**:
  - Gemini AI integration (gemini-2.0-flash model)
  - Vehicle questionnaire analysis for accurate tracking initialization
  - Vietnamese prompt engineering for token optimization
  - AI usage tracking (tokens, cost, response time)
- **Responsibilities**:
  - Analyze user questionnaire responses about vehicle condition
  - Estimate last replacement dates and predict next service dates
  - Provide confidence scores and reasoning for recommendations

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
5. **AI-assisted onboarding** - Gemini integration for vehicle maintenance tracking initialization
6. **Domain redesign** - Renamed entities (ConsumableItem → PartCategory, StandardMaintenanceSchedule → DefaultMaintenanceSchedule)

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

## AI-Assisted Vehicle Onboarding Flow

The system implements an optional AI-assisted onboarding process to accurately initialize vehicle maintenance tracking:

### Problem
When users register a vehicle, the system doesn't know if parts (like engine oil) were replaced before registration. Using default "new vehicle" assumptions leads to inaccurate tracking until the first manual maintenance record.

### Solution
AI analyzes user responses to part-specific questionnaires to estimate the actual maintenance state.

### Flow Steps

1. **Create Vehicle** (Required)
   - User creates UserVehicle via POST `/api/v1/user-vehicles`
   - System sets `NeedsOnboarding = true`
   - All part trackings initialized with default manufacturer schedules (no LastReplacement dates)

2. **Get Part Schedule** (Optional - Per Part)
   - Frontend calls GET `/api/v1/vehicle-models/{vehicleModelId}/part-categories/{partCategoryCode}/default-schedule`
   - Returns manufacturer's recommended schedule for specific part (e.g., engine_oil)
   - Example: "First 1000km, then every 5000km or 6 months"

3. **Show Questionnaire** (Optional - Per Part)
   - Frontend displays part-specific questionnaire
   - Questions configured by frontend (e.g., "Khi nào thay nhớt gần nhất?", "Xe đã chạy bao nhiêu km khi thay?")
   - User can skip questionnaire for any part

4. **AI Analysis** (Optional - Per Part)
   - Frontend sends to POST `/api/v1/ai/vehicle-questionnaire/analyze`
   - Request includes:
     - Vehicle info (brand, model, current odometer, purchase date)
     - Default schedule for ONE part only (validated)
     - User's questionnaire answers
   - AI (Gemini 2.0-flash) analyzes and returns:
     - Estimated LastReplacementOdometer and LastReplacementDate
     - Predicted PredictedNextOdometer and PredictedNextDate
     - Confidence score (0.0-1.0) and reasoning

5. **Review & Apply** (Optional - Per Part)
   - Frontend shows AI recommendation to user
   - If user confirms, frontend calls POST `/api/v1/user-vehicles/{id}/apply-tracking`
   - System updates VehiclePartTracking with AI recommendations
   - Stores AI reasoning and confidence score for audit trail

6. **Repeat or Skip** (Optional)
   - User can repeat steps 2-5 for other parts (oil_filter, air_filter, etc.)
   - Or skip remaining parts entirely
   - Skipped parts keep default tracking (no LastReplacement, uses manufacturer schedule)

7. **Complete Onboarding** (Optional)
   - User finishes onboarding (may have analyzed 0, some, or all parts)
   - Frontend can update `NeedsOnboarding = false` via PUT `/api/v1/user-vehicles/{id}`

### Key Design Decisions

- **Single-Part Analysis**: AI analyzes ONE part per request for accuracy (validated on both request and response)
- **Partial Onboarding**: Users can skip questionnaires; unanalyzed parts use default schedules
- **Optional Flow**: Users can skip entire onboarding and rely on manufacturer defaults
- **Vietnamese Prompts**: AI prompts optimized in Vietnamese to reduce token costs
- **Audit Trail**: AI reasoning stored in `VehiclePartTracking.AiAnalysisResult` as JSON

### Key Endpoints

- `GET /api/v1/vehicle-models/{vehicleModelId}/part-categories/{partCategoryCode}/default-schedule` - Get manufacturer schedule for specific part
- `POST /api/v1/ai/vehicle-questionnaire/analyze` - AI analyzes questionnaire (1 part only)
- `POST /api/v1/user-vehicles/{id}/apply-tracking` - Apply AI recommendation to part tracking

## Notes for AI Assistants

- This is a greenfield .NET Aspire project following modern practices
- Services use clean architecture except Identity (simpler monolithic design)
- Focus on domain-driven design and separation of concerns
- Event-driven patterns used for cross-service communication
- Security-conscious: no secrets in source control
- **Minimal API pattern**: Use Minimal API endpoints, NOT Controllers
- **Bootstrapping pattern**: Use extension methods in `Bootstrapping/` folders for DI registration
- **Mapping pattern**: Use static extension methods for DTO transformations
