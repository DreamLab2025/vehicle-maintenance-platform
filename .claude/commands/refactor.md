Refactor the following code in the Verendar backend: $ARGUMENTS

Read the target code first before proposing any changes. Understand why it exists before changing it.

## Refactoring goals
- Improve clarity and maintainability — not just aesthetics
- Remove duplication only when the abstraction is clearly named and reused 3+ times
- Do not add complexity for hypothetical future needs

## Constraints
- Do not change public API contracts (method signatures, route shapes, response shapes)
- Do not introduce AutoMapper — keep static extension methods
- Do not move to Controllers — stay on Minimal API
- Preserve all existing behavior — refactoring must be semantics-preserving
- If splitting a class, keep each piece in the correct CA layer

## Output format
1. What the current code does (briefly)
2. What specific problems you're fixing (name them)
3. The refactored code
4. What changed and why
