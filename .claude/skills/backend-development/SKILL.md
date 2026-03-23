---
name: backend-development
description: >
  Verendar .NET 9 backend development patterns for building features, endpoints, and services.
  Use this skill whenever the user asks to add an endpoint, service, domain entity, migration,
  repository, or any backend feature in the Verendar microservices platform. Also load this skill
  for code reviews, refactors, debugging, or questions about project architecture. If the user
  mentions any service (Identity, Vehicle, Media, Notification, Ai, Location), API route, or
  .NET/Aspire/EF Core concept within this repo, this skill should be active.
---

# Verendar Backend Development

This skill covers the patterns and conventions for the Verendar platform. Always read the relevant
reference file before implementing a feature — it contains real code patterns extracted from the
codebase.

## Reference Files

| Need | File |
|------|------|
| API routes, minimal API handlers, pagination, response envelope | `references/api-design.md` |
| Layer responsibilities (Host/Application/Domain/Infrastructure) | `references/architecture.md` |
| Mapping extensions, service patterns, SOLID practices | `references/code-quality.md` |
| Redis caching strategy, N+1 prevention, query hygiene | `references/performance.md` |
| Unit tests: xUnit + Moq + FluentAssertions | `references/testing.md` |
| HTTP clients (IHttpClientFactory, BaseServiceClient, Polly) and RabbitMQ/MassTransit (events, consumers, publish) | `references/communication.md` |

## Quick Decision Guide

- **Adding an endpoint?** → read `api-design.md`
- **Where does this code live?** → read `architecture.md`
- **Writing a service or mapping?** → read `code-quality.md`
- **Caching or query optimization?** → read `performance.md`
- **Writing tests?** → read `testing.md`
- **Calling another service (HTTP) or publishing/consuming events (RabbitMQ)?** → read `communication.md`

## Core Invariants (never violate)

- No controllers — Minimal API with `MapGroup` only
- No AutoMapper — hand-written `ToResponse()` / `ToEntity()` extension methods
- No MediatR — services call `IUnitOfWork` directly
- All endpoints return `ApiResponse<T>` (except internal service-to-service routes and AI health check)
- Soft delete only: `DeletedAt = DateTime.UtcNow`, never `DbContext.Remove()`
- Primary constructors with `private readonly` fields throughout
- `async/await` everywhere — never `.Result` or `.Wait()`
- Secrets via User Secrets — never hardcode in `appsettings.json`

## Services Overview

| Service | DB | Cache | Queue | Has Tests |
|---------|---|-------|-------|-----------|
| Identity | identity-db | Redis | RabbitMQ | — |
| Vehicle | vehicle-db | — | RabbitMQ | — |
| Media | media-db | — | RabbitMQ | — |
| Notification | notification-db | — | RabbitMQ | — |
| Ai | ai-db | — | RabbitMQ | — |
| Location | location-db | Redis | RabbitMQ | ✓ reference |

The **Location service** is the canonical reference for caching patterns and unit tests.
