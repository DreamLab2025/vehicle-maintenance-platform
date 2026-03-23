Work with CI for the current branch. Arguments: $ARGUMENTS (e.g. "fix failing build", "run locally", "add test job")

## Run CI locally
```bash
task app:build
dotnet test Location/Verendar.Location.Tests
```

## Fix a failing CI run
1. Reproduce locally: build + test
2. Identify root cause from logs/exit code
3. Apply minimal fix if asked

## Add or change a workflow
1. Read `.github/workflows/*.yml` for existing patterns
2. Edit/add workflow (triggers: `push`/`pull_request` on main; steps: dotnet build + test)
3. Use repo secrets for credentials — never log token values
4. Pin actions by tag (e.g. `actions/checkout@v4`); add `concurrency` group to cancel duplicate runs
