---
description: Restate requirements, assess risks, and create step-by-step implementation plan. WAIT for user CONFIRM before touching any code.
---

# Plan Command

This command invokes the **planner** agent to create a comprehensive implementation plan before writing any code.

## What This Command Does

1. **Restate Requirements** - Clarify what needs to be built
2. **Identify Risks** - Surface potential issues and blockers
3. **Create Step Plan** - Break down implementation into phases
4. **Wait for Confirmation** - MUST receive user approval before proceeding

## When to Use

Use `/ck:plan` when:

- Starting a new feature
- Making significant architectural changes
- Working on complex refactoring
- Multiple files/services will be affected
- Requirements are unclear or ambiguous

## How It Works

The planner agent will:

1. **Analyze the request** and restate requirements in clear terms
2. **Break down into phases** following the Verendar layer order: Domain → Infrastructure → Application → Host → Tests
3. **Identify dependencies** between components and services
4. **Assess risks** and potential blockers
5. **Estimate complexity** (High/Medium/High)
6. **Present the plan** and WAIT for your explicit confirmation

## Example Usage

```
User: /ck:plan Add push notification when a booking is assigned to a mechanic

Agent (planner):
# Implementation Plan: Booking Assigned Push Notification

## Requirements Restatement
- When a booking is assigned to a mechanic, send a push notification to the mechanic
- Notification must include booking ID, vehicle info, and scheduled time
- Delivered via existing Notification service
- Triggered by BookingAssignedEvent (MassTransit/RabbitMQ)

## Implementation Phases

### Phase 1: Contracts
- Ensure `BookingAssignedEvent` in `Verendar.Garage.Contracts` includes MechanicId, BookingId, VehicleInfo, ScheduledAt
- If missing fields, add them and update the publisher

### Phase 2: Notification Consumer
- Add consumer `BookingAssignedNotificationConsumer` in `Notification/Verendar.Notification.Infrastructure/Consumers/`
- Consume `BookingAssignedEvent`; look up mechanic's device token via `IIdentityClient`
- Call push notification provider (FCM/APNs) or existing `IPushService`

### Phase 3: Application Service
- Add `SendBookingAssignedNotificationAsync` to `NotificationService`
- Build notification payload: title "Booking Assigned", body includes vehicle + time
- Persist notification record in `Notifications` table (soft-delete compliant)

### Phase 4: Host Wiring
- Consumer is auto-discovered — no manual registration needed
- Verify consumer appears in Aspire dashboard under Notification service

### Phase 5: Tests
- Unit test `BookingAssignedNotificationConsumer`: mock `IIdentityClient`, mock `IPushService`
- Verify notification record persisted on success
- Verify no crash if mechanic device token is null (graceful skip)

## Dependencies
- `BookingAssignedEvent` must carry `MechanicId`
- `IIdentityClient` must expose `GetDeviceTokenAsync(userId)`
- Push service integration must already exist or be added in Phase 3

## Risks
- HIGH: If mechanic has no device token registered, skip silently — don't fail the consumer
- MEDIUM: `IIdentityClient` call adds latency — consider caching token per userId (Redis, TTL 1h)
- LOW: Consumer idempotency — MassTransit retries on failure; notification must not duplicate

## Estimated Complexity: MEDIUM

**WAITING FOR CONFIRMATION**: Proceed with this plan? (yes/no/modify)
```

## Important Notes

**CRITICAL**: The planner agent will **NOT** write any code until you explicitly confirm the plan with "yes", "proceed", or similar affirmative response.

If you want changes, respond with:

- `"modify: [your changes]"`
- `"different approach: [alternative]"`
- `"skip phase 2 and do phase 3 first"`

## Integration with Other Commands

After planning and confirming:

- Use `/ck:cook` to implement the plan
- Use `/ck:test` to run tests after implementation
- Use `/ck:git:cm` to commit when done

## Related Agent

This command invokes the `planner` agent defined at `.claude/agents/planner.md`.
