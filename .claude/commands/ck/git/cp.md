Stage all changed files, create a commit, and push to the current remote branch.

1. Run `git status` to see what changed
2. Run `git diff` to understand the actual changes
3. Stage relevant files (avoid secrets, generated files, bin/obj)
4. Write a commit message following the project convention (`feat:`, `fix:`, `refactor:`, `chore:`, `docs:`)
5. Commit
6. Push to `origin` on the current branch

Confirm the push succeeded and show the remote URL.

Do not force-push. Do not commit secrets or build artifacts.
