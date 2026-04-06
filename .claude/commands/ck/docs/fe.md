Generate a frontend-facing API change document for the current branch.

## Purpose

Produce a concise, structured doc that a frontend developer can read to know:

- What endpoints changed (new / modified / removed)
- Exact request shapes and response shapes
- Auth requirements (roles, bearer token)
- Any breaking changes

## Step 1 — Identify changed API surface

```bash
git diff main...HEAD --name-only
git log --oneline main...HEAD
```

Focus on files under:

- `{Service}/Verendar.{Service}/Apis/` — endpoint definitions
- `{Service}/Verendar.{Service}.Application/` — DTOs, request/response models
- `{Service}/Verendar.{Service}.Domain/` — constants, entity fields

## Step 2 — Read each changed endpoint file

For every changed `*Apis.cs` file, read it to extract:

- HTTP method + route pattern
- Auth requirements (`.RequireAuthorization(...)`)
- Request model (query params, route params, body type)
- Response type returned by the handler

Also read the corresponding service files to get the full response shape.

## Step 3 — Write the document

Output to `docs/frontend/YYYY-MM-DD-<branch-slug>.md` (create `docs/frontend/` if absent).

Use the structure below. Only include sections that apply.

---

````markdown
# Frontend API Changes — <branch or feature name>

**Date**: YYYY-MM-DD
**Branch**: <current branch>
**Status**: Draft

## Summary

One-paragraph description of what this feature does from the frontend's perspective.

## New Endpoints

### `METHOD /api/route`

**Auth**: Bearer JWT required · Roles: `RoleA`, `RoleB`

**Request**

```json
// body / query params
{
  "field": "type — description"
}
```
````

**Success Response** `200 OK`

```json
{
  "isSuccess": true,
  "data": { ... }
}
```

**Error Responses**
| Status | When |
|--------|------|
| 400 | invalid input |
| 401 | missing/invalid token |
| 403 | insufficient role |
| 404 | resource not found |

---

## Modified Endpoints

List endpoints that existed before but changed behavior, request shape, or response shape.
Call out **breaking changes** explicitly:

> ⚠ Breaking: `fieldName` renamed to `newFieldName`

---

## Removed Endpoints

List any endpoints that were deleted. Suggest the replacement if one exists.

---

## Constants / Enums

List any new or changed string constants the frontend needs to handle
(statuses, kinds, event types, etc.).

| Name     | Values              |
| -------- | ------------------- |
| `Status` | `active`, `pending` |

---

## Notes for Frontend

Any integration gotchas, pagination behavior, file upload requirements, etc.

```

## Rules

- Use actual field names from the code — do not invent or paraphrase
- Response shapes come from the DTO/handler, not from imagination
- If a field is optional, mark it `?`
- Do not document endpoints that did not change
- If `$ARGUMENTS` is provided, use it as the feature name / focus area; otherwise derive from branch name
```
