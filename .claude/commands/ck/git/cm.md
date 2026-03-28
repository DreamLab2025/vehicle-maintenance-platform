Stage all changed files and create a git commit with a well-formed commit message.

1. Run `git status` to see what changed
2. Run `git diff` to understand the actual changes
3. Run `git log --oneline -5` to match existing commit message style
4. Stage relevant files (avoid secrets, generated files, bin/obj)
5. Write a commit message that:
   - Starts with a type prefix: `feat:`, `fix:`, `refactor:`, `chore:`, `docs:`
   - Summarizes *what changed and why* in one line (under 72 chars)
   - Adds a body paragraph if the change is non-obvious

Do not commit: `.env`, `*.user`, `appsettings.*.json` with secrets, `bin/`, `obj/`
