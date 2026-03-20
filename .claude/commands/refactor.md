Refactor the following code in the Verendar backend: $ARGUMENTS

Read the target code first. Understand why it exists before changing it.

## Goals
- Improve clarity and maintainability — not aesthetics
- Remove duplication only when the abstraction is clearly named and reused 3+ times
- No complexity for hypothetical future needs

## Constraints
- Preserve all existing behavior
- No public API contract changes (method signatures, route shapes, response shapes)
- No AutoMapper, no Controllers, no MediatR — see CLAUDE.md
- Keep each piece in the correct CA layer

## Output
1. What the current code does (briefly)
2. What specific problems you're fixing
3. The refactored code
4. What changed and why
