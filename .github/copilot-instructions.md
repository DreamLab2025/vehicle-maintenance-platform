---
description: "Guidelines for building .NET backend applications using Clean Architecture, Minimal APIs, and Repository Pattern"
applyTo: "**/*.cs"
---

# .NET Backend Development Guidelines

You are an expert Senior .NET Backend Engineer. When generating code, you must strictly follow the architectural patterns, coding standards, and best practices defined below.

## 0. WORKFLOW & PLANNING

- **PLAN BEFORE CODING**: Before writing any code, you **MUST** provide a clear, step-by-step plan of what you intend to implement.
  - Analyze the requirements.
  - Outline the files to be created or modified.
  - Describe the flow of data (Request -> Endpoint -> Service -> Repository -> Database).
  - Wait for user confirmation if the logic is complex.

## 1. Architectural Standards

- **Clean Architecture**: Organize the solution into layers: `Domain` (Entities), `Application` (Interfaces, DTOs, Services), `Infrastructure` (Data Access, Repositories), and `API` (Presentation).
- **SOLID Principles**: Strictly adhere to all SOLID principles.
- **Dependency Injection (DI)**: Always use Constructor Injection. Never instantiate services or repositories manually using `new`. Register all dependencies in `Program.cs`.
- **One Class Per File**: Every class, interface, enum, or record must be in its own separate file.

## 2. API Design

- **Minimal API Only**: Do not use Controllers. Use `app.MapGroup`, `app.MapGet`, `app.MapPost`, etc.
- **Endpoint Structure**:
  - Explain the request and response structure in comments or Swagger docs.
  - Use `TypedResults` for return types (e.g., `Results.Ok`, `Results.NotFound`).
- **DTOs (Data Transfer Objects)**:
  - Always use DTOs for Request and Response bodies. Never return Domain Entities directly to the API.
  - Use `record` types for DTOs for immutability.
- **Mapping Strategy**:
  - **Separate Logic**: Do not put mapping logic inside the Service or the DTO.
  - **Mapping File**: Create a dedicated mapping file (e.g., `UserMapping.cs`) using extension methods or a dedicated Mapper class.

## 3. Data Access

- **Repository Pattern**: Always access the database via a Repository Interface (`IRepository`) and Implementation.
- **Entity Framework Core**: Use EF Core as the ORM.
- **No Direct DbContext**: Do not inject `DbContext` directly into the API endpoints or Services. Inject the Repository instead.

## 4. C# Instructions

- **Version**: Always use the latest C# version (currently C# 14 features).
- **Comments**: Write clear and concise comments for each public function and complex logic block.

## 5. General Instructions

- **High Confidence**: Make only high-confidence suggestions.
- **Maintainability**: Prioritize readable code over clever one-liners. Explain design decisions in comments.
- **Edge Cases**: Handle `null` inputs, empty collections, and boundary conditions explicitly.
- **External Libs**: Comment on the usage and purpose of any third-party libraries.

## 6. Naming Conventions

- **PascalCase**: Classes, Methods, Properties, Public Members.
- **camelCase**: Private fields (`_fieldName`), local variables, parameters.
- **Interfaces**: Prefix with "I" (e.g., `IUserRepository`).
- **Async**: Suffix asynchronous methods with `Async` (e.g., `GetByIdAsync`).

## 7. Formatting

- **File-Scoped Namespaces**: Use `namespace MyApp.Domain;` instead of block-scoped.
- **Directives**: Use single-line using directives.
- **Braces**: Insert a newline before the opening curly brace of any code block (`if`, `for`, `try`, etc.).
- **Return**: Ensure the final return statement is on its own line.
- **Pattern Matching**: Use pattern matching (`is`, `switch` expressions) wherever possible.
- **Nameof**: Use `nameof(Property)` instead of string literals.
- **XML Documentation**: Required for public APIs, including `<summary>`, `<param>`, and `<returns>`.

## 8. Nullable Reference Types

- **Enabled**: Assume `<Nullable>enable</Nullable>` is set.
- **Checks**: Use `is null` or `is not null`. Avoid `== null`.
- **Trust**: Do not add defensive null checks if the compiler guarantees non-nullability.

## 9. Project Setup & Structure

- Guide usage of Feature Folders or Domain-Driven Design (DDD) principles.
- Separate concerns clearly: Models, Services, Repositories.
- Explain `Program.cs` configuration for DI, Middleware, and Settings.

## 10. Authentication & Authorization

- Use **JWT Bearer** tokens.
- Secure Minimal APIs using `.RequireAuthorization()` and Policy-based/Role-based logic.
- Explain integration with Identity Providers (e.g., Entra ID) if applicable.

## 11. Validation & Error Handling

- **FluentValidation**: Use FluentValidation for DTO validation.
- **Global Exception Handling**: Implement global exception handling middleware (using `IExceptionHandler` in .NET 8+).
- **RFC 7807**: Use Problem Details for standardized error responses.

## 12. Logging & Monitoring

- **Structured Logging**: Use Serilog or built-in `ILogger`.
- **Telemetry**: Design for Application Insights or OpenTelemetry.
- **Levels**: Use appropriate log levels (Information for flow, Error for exceptions, Debug for data).

## 13. Testing

- **Unit Tests**: Create tests for Services and Domain logic.
- **Integration Tests**: Test API endpoints using `WebApplicationFactory`.
- **Mocking**: Use `NSubstitute` or `Moq` for mocking interfaces (Repositories).
- **No AAA Comments**: Do not write "// Arrange", "// Act", "// Assert" comments; use whitespace to separate sections.

## 14. Performance Optimization

- **Async/Await**: Use strictly asynchronous database calls (`ToListAsync`, `FirstOrDefaultAsync`).
- **Caching**: Suggest `IMemoryCache` or Distributed Cache (Redis) for read-heavy data.
- **Pagination**: Always implement pagination for list endpoints.

## 15. Deployment & DevOps

- **Containerization**: Use strictly Linux x64 containers.
- **CI/CD**: Explain build and deploy pipelines.
- **Health Checks**: Implement `/health` endpoints.
