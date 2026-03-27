Analyze CI/CD logs and fix the failing build or test run: $ARGUMENTS

Accepts a GitHub Actions run URL (e.g. `https://github.com/owner/repo/actions/runs/123456`) or a description of the failure.

## Step 1 — Get the logs
If a URL is provided, fetch the run details:
```bash
gh run view <run-id> --log-failed
```
Or navigate to the URL and read the failure output from the relevant job step.

## Step 2 — Classify the failure
- **Build error** — compilation failure; treat as `/fix:build`
- **Test failure** — one or more tests failed; read the assertion output carefully
- **Workflow config error** — YAML syntax, missing secret, wrong action version
- **Flaky test** — test passed locally, fails in CI due to timing or environment

## Step 3 — Fix
- For build errors: fix the compilation problem, do not skip or suppress errors
- For test failures: trace to root cause — implementation bug or outdated test expectation
- For workflow config: edit `.github/workflows/*.yml` using existing workflows as pattern
- For flaky tests: identify the non-determinism (missing seed, race condition, order dependency) and fix the test setup

## Step 4 — Verify locally
```bash
task build
task test
```
Confirm clean before pushing.

Never use `--no-verify` or skip hooks to force a broken commit through.
