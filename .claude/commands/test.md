Run the test suite for the Verendar backend.

## Default: run all tests
```bash
task test:all
```

## Run a specific service's tests
```bash
task test PROJECT=Location/Verendar.Location.Tests
task test PROJECT=Garage/Verendar.Garage.Tests
task test PROJECT=Vehicle/Verendar.Vehicle.Tests
```

## Run with coverage
```bash
task test:coverage PROJECT=Location/Verendar.Location.Tests
```

## After tests run

1. If all pass — confirm count and report clean.
2. If any fail — read the failure output carefully:
   - Is the test assertion wrong (expected changed), or the implementation broken?
   - Trace the failing test to the service/validator it covers
   - Do not delete or weaken tests to make them pass
3. If asked to fix failures, use `/fix:test` for details.

## Interpreting results
- Test projects live at `{Service}/Verendar.{Service}.Tests/`
- Tests use xUnit + Moq + FluentAssertions — see `.claude/skills/backend-development/references/testing.md`
- Location service is the reference test implementation
- If tests fail with connection errors, check Docker or Aspire infrastructure is running (`task docker:dev:up`)
