# Backend Mindset (2025)

## The Foundation: Think Before You Code

The best backend engineers spend as much time thinking as coding. A 30-minute design conversation prevents days of rework. Before writing any code, make sure you can answer:

- What problem does this solve, for whom?
- What are the edge cases and failure modes?
- How will this be tested and monitored?
- How will this change in 6 months?

If you can't answer these, the code will reflect that uncertainty.

---

## Domain-First Thinking

**Start with the domain, not the database.** The instinct to open a new table migration before defining the entity in code is backwards. The database is an implementation detail — the business concept is the thing.

Questions to ask first:
- What is this entity *called* in the domain? What does the business team call it?
- What does this entity *know* about itself? What are its invariants?
- What does this entity *do*? What state transitions does it go through?

Only after these answers does a schema make sense. The schema serves the domain, not the other way around.

**Rich domain vs. anemic domain**: An entity that's just a bag of properties with all logic in services is an anemic domain model. Logic that only makes sense in the context of that entity should live in the entity — it's more discoverable and harder to misuse.

---

## Lazy Creation — Default to Not Creating

Don't pre-allocate records on the assumption they'll be needed. Create data when the user explicitly requests it.

**The cost of premature creation**: phantom records that represent things that never happened, state machines with "empty" initial states that mean nothing, data that needs to be cleaned up because most of it was never used.

**The Verendar example**: VehiclePartTracking records are only created when a user explicitly chooses to analyze a part. Not when the vehicle is registered. Not for every possible part category. Only for what the user actually engages with. This means the data is always meaningful.

**Apply this thinking to**: default records created "just in case", initialization data created before the user needs it, prefetching data that most users won't see.

---

## Granularity — One Thing Per Operation

Operations that do one focused thing are easier to retry, debug, test, and understand. Operations that bundle multiple concerns together fail in ambiguous ways.

**Signals you're doing too much in one operation**:
- The method name uses "and" (`CreateVehicleAndInitializeTracking`)
- The response shape is inconsistent with what was requested
- Partial failures are possible and unclear to handle
- The method is hard to name without listing everything it does

**The AI analysis example**: each request analyzes exactly one part. Not "analyze all parts." Not "analyze and apply." Just analyze one part. The caller decides what to do with the result. This makes retry logic trivial and partial success impossible to confuse.

---

## Selective Design — Users Choose What Matters

Don't impose a fixed, universal model on all users. Design for selective engagement.

Users have different needs. Some want to track every maintenance item obsessively. Others just want basic reminders for oil changes. A system that forces all-or-nothing engagement loses both groups.

**Design implications**:
- Make features opt-in, not opt-out
- Never assume a record must exist for every possible case
- Support "I'll come back and add this later" patterns
- Measure what users actually engage with — that's the real feature set

---

## Audit and Traceability — Store the "Why"

When a system makes decisions — especially automated or AI-driven ones — store the reasoning alongside the result. The result alone becomes a mystery when something goes wrong.

**What to store**:
- Who triggered the action (userId, system job name)
- When it happened (timestamps)
- Why (AI confidence score, reasoning, source data used)
- What was the input that led to this output

This isn't just for debugging. It's for user trust: "the system said my engine oil is due next month — here's why it thinks that." Traceable reasoning builds trust in automation.

---

## Security as a Starting Point, Not an Afterthought

Security decisions made early are cheap. Security incidents are expensive.

**Default posture**:
- Endpoints are authenticated by default — explicitly opt out for public routes
- Resources are owned — verify that the requesting user owns the resource before any operation
- Input is untrusted — validate everything at the boundary, sanitize appropriately
- Secrets are external — never in source, never in logs

**The ownership check** is the most commonly missed: authenticated ≠ authorized for this specific resource. A user who is logged in can still request another user's data if you don't check.

**Secrets hygiene as a habit**: review `appsettings.json` in every PR. If a value looks like it could be a credential, it doesn't belong there.

---

## Architectural Decision Framework

Use this sequence when designing something new:

1. **Domain boundary** — which service owns this? Does it cross service boundaries?
2. **Communication pattern** — sync HTTP for user-facing reads/writes; async events for cross-service side effects
3. **Data model** — new entity or extension of an existing one? What are the invariants?
4. **Creation strategy** — lazy or eager? Default to lazy.
5. **Storage vs. computation** — store if expensive to recompute or needed for audit. Compute if trivial and always fresh.
6. **Failure modes** — what happens if this call fails? Can the caller retry? Is the operation idempotent?

---

## When Patterns Don't Fit — Deviate with Intention

Patterns are tools for recurring problems. When the problem doesn't match, the pattern doesn't apply.

**Legitimate deviations in this project**:
- Identity service is a single project — CA layers would add ceremony without value at its complexity level
- Cross-service HTTP for lookups — eventual consistency is wrong when you need the email immediately

**The discipline of deviation**: when you break a pattern, document why. A comment or ADR (Architecture Decision Record) that says "we chose X over Y because Z" is worth its weight. Future engineers won't have to reverse-engineer your reasoning, and won't accidentally "fix" something that was intentionally different.

Good engineering isn't following rules — it's knowing when the rules apply and having the judgment to adapt when they don't.
