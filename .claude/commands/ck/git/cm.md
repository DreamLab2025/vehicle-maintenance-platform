Stage all changed files and create a git commit with a well-formed commit message.

1. `git status` — see what changed
2. `git diff` — understand the actual changes
3. `git log --oneline -5` — match existing commit message style
4. Stage relevant files (skip: `.env`, `*.user`, `appsettings.*.json` with secrets, `bin/`, `obj/`)
5. Commit with message:
   - Type prefix: `feat:`, `fix:`, `refactor:`, `chore:`, `docs:`
   - One line summary of what changed and why (under 72 chars)
   - Body paragraph if the change is non-obvious
