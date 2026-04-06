Research and create an implementation plan for: $ARGUMENTS

---

## Flags

| Flag         | Effect |
| ------------ | ------ |
| `--fast`     | Shallow scout, skip Steps 5–6. Best for simple CRUD. |
| `--hard`     | Deep scout across all layers. Force Steps 5 + 6 with more detail. |
| `--auto`     | No pauses between steps. Default. |
| `--parallel` | Run Scout + Analyze concurrently. |
| `--two`      | Fork at Step 3: produce Approach A + Approach B + Recommendation. |
| `--no-tasks` | Do not emit a TodoWrite block. |

Default (no flags): run Steps 1–4; include Steps 5–6 only when schema or auth changes are present.

Subcommands: `/ck:plan:archive`, `/ck:plan:validate`, `/ck:plan:red-team`

---

## Step 1 — Scout

Invoke `/ck:scout` with the feature name/keywords.

Find: closest existing feature, entities/repos/services involved, naming and response shape conventions.

Also check `docs/requirements/`, `docs/architecture/`, `docs/design/`, `docs/plans/` for documented constraints or prior plans.

**`--fast`**: single targeted query, stop at first match.
**`--hard`**: full call-chain scout (endpoint → handler → service → repo → DB config → migration).
**`--parallel`**: launch alongside Step 2.

---

## Step 2 — Analyze

- **Service**: which Verendar microservice owns this? (Identity · Vehicle · Media · Notification · Ai · Garage · Payment · Location)
- **Entities**: list each entity/table touched — read, written, or new
- **Current state**: what exists vs. what must be added
- **Cross-service**: does this require async events (MassTransit) or sync HTTP calls (typed client)?
- **Assumptions**: call out anything inferred — flag explicitly
- **Open questions**: anything that blocks design

**`--hard`**: use `sequential-thinking` skill; also identify what adjacent features might break.

---

## Step 3 — Design

**API Contract**: route, HTTP method, request/response shapes, status codes per case, required auth/roles.

**Data Flow**:
```
Request → Validator → Handler → Service → Repository → DB
                                        ↓
                                   Response shape
```
Note async side-effects (MassTransit events, cache invalidation, media ops).

**Interfaces**: new repository method signatures, new service method signatures, new/changed DTOs.

**`--two`**: fork here — produce Approach A + Approach B, each with full API Contract / Data Flow / Interfaces, then add a Recommendation section.

---

## Step 4 — Plan

Ordered, layer-by-layer steps. Each step names the exact file and what changes.

```
N. [Layer] File — what to add/change
   Depends on: (step numbers, or "none")
```

Layer order: **Domain → Application → Infrastructure → Api**

Include: new entities/fields, repo interface + impl, service method + validator, endpoint handler + route, `GlobalUsings.cs` updates, migration, MassTransit contracts/consumers if needed.

**Without `--no-tasks`**: emit a TodoWrite block with each step as a task.

---

## Step 5 — Validate _(skip with `--fast`; mandatory with `--hard` or schema/auth changes)_

- Migration safety: NOT NULL columns have defaults or are nullable
- Auth: endpoint is properly protected; ownership checks are inside the service
- No N+1 introduced
- Response shape matches `ApiResponse<T>` contract
- Pagination used for all list endpoints (except Location unbounded lists)

---

## Step 6 — Red-team _(skip with `--fast`; mandatory with `--hard`)_

Adversarially challenge the plan. For each: state the failure mode and whether the plan mitigates it.

- Permission/ownership edge cases
- Concurrent mutation / race conditions
- Invalid state transitions
- Migration rollback safety
- Missing business rule not enforced
- Cross-service consistency (event vs. sync call tradeoffs)

---

Do not write implementation code — only the plan.
Flag any assumption that needs confirmation before implementation begins.
