---
name: code-review
description: >
  Rigorous code review discipline: receiving technical feedback without performative agreement,
  requesting systematic independent review (subagent / second pass), and verification gates before
  claiming completion. Use this skill whenever the user asks for a review, PR feedback, "LGTM",
  quality check, pre-merge gate, or when you are about to mark work done after substantive edits.
  Also use when the user shares reviewer comments, security concerns, or asks "is this good enough"
  — apply evidence-based judgment, not reassurance. For Verendar backend specifics after loading
  this skill, pull `backend-development` references (code-quality, api-design) as needed.
---

# Code Review

Three habits: **how you take feedback**, **how you ask for review**, **how you prove done**.

## References

| Situation              | File                          |
| ---------------------- | ----------------------------- |
| Cold-read without Task | `references/review-checklist.md` |

---

## 1. Receiving feedback — technical evaluation over performative agreement

When the user (or another agent) gives review comments, critique, or pushback:

- **Parse the claim.** What exactly is being asserted (bug, style, security, API contract, performance)?
- **Verify against the codebase.** Open the cited paths; trace call paths; check assumptions. Do not agree because the tone was confident or authoritative.
- **Separate preference from defect.** Naming or formatting preferences matter less than correctness, security, data integrity, and public contracts. Say which category each point falls into.
- **Respond with evidence.** For each point: agree / disagree / partially agree, with **file references or behavior** (what you read, what you ran). If you disagree, state why with a concrete reason, not deflection.
- **Avoid performative agreement.** Do not say "you're right" or "great catch" unless you have validated the issue. It is fine to say "I'll verify that" and then verify before conceding.

---

## 2. Requesting reviews — systematic review via subagent

Before treating a non-trivial change as ready (especially multi-file, auth, data, or API surface):

- **Do not self-certify as the only reviewer.** After your implementation pass, schedule a **second, independent review pass**.
- **How to delegate (Cursor):** use the **Task** tool with `subagent_type` `generalPurpose` (or the most capable review-oriented option available). Prompt the subagent explicitly as a **code reviewer**, not an implementer:
  - Scope: list paths or diff summary; forbid scope creep and new features.
  - Ask for: correctness, edge cases, security/auth, contract stability, regressions, test gaps.
  - Require output grouped by **Must fix / Should fix / Consider** (skip empty groups).
- **If no Task tool:** simulate a cold read — re-read changed files as if you did not write them; use a checklist (see `references/review-checklist.md`).
- **Incorporate findings** using section 1 (verify each item; do not rubber-stamp).

---

## 3. Verification gates — evidence before completion claims

Do not state that work is **complete**, **fixed**, **passing**, or **ready to merge** without **artifacts**:

| Claim | Minimum evidence |
| ----- | ---------------- |
| Builds | Command run + success (e.g. `dotnet build`, `task app:build`) or exact error if failed |
| Tests | Test command + outcome; if no tests exist, state that explicitly |
| Behavior | Repro steps or the code path (file:line) that implements the behavior |
| Regression avoided | What you checked (related handler, consumer, migration) |

- **Prefer running commands** in the real environment when the user expects a working tree (per user rules).
- **If you cannot run** (sandbox, missing deps): say what was **not** verified and what remains manual.
- **Forbidden phrasing** without backing evidence: "should work", "looks good", "all set", "CI will pass" — replace with what was actually run or read.

---

## Verendar backend cross-reference

For layer rules, `ApiResponse<T>`, Minimal API, EF, and tests — after applying this skill’s process, load **backend-development** and the relevant reference (`code-quality.md`, `api-design.md`, etc.) so findings align with repo conventions.

---

## Quick decision

```
User shares PR comments or "review this"?
  → §1: verify each point; evidence-based reply.

You finished a multi-file / risky change?
  → §2: subagent review pass; then §3 gates.

You want to say "done"?
  → §3: build/tests/trace — only then claim completion.
```
