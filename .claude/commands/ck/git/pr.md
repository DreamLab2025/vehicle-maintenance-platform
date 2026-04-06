Create a pull request for the current branch. Arguments: $ARGUMENTS (optional: target branch, defaults to main)

1. `git log main..HEAD --oneline` — see all commits in this branch
2. `git diff main...HEAD` — understand all changes
3. Push to origin if not already pushed
4. `gh pr create` with:
   - **Title**: imperative, under 70 chars (e.g., `feat: add project CRUD endpoints`)
   - **Body**:
     - `## What` — what changed (bullets)
     - `## Why` — business/technical reason
     - `## How to test` — steps to verify

Return the PR URL when done.
