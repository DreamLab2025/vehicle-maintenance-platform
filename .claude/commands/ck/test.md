Explicitly run the test suite. Use when you need integration tests, a full suite run before commit/PR, or targeted filtering.

```bash
task test:all                                         # all services
task test PROJECT=Garage/Verendar.Garage.Tests        # single service
task test PROJECT=Identity/Verendar.Identity.Tests
task test PROJECT=Vehicle/Verendar.Vehicle.Tests
task test PROJECT=Ai/Verendar.Ai.Tests
task test PROJECT=Location/Verendar.Location.Tests
task test PROJECT=Media/Verendar.Media.Tests
task test PROJECT=Notification/Verendar.Notification.Tests
task test PROJECT=Payment/Verendar.Payment.Tests
```

## Interpreting results

- **All pass** → confirm count, report clean
- **Any fail** → read failure output: is the assertion wrong or the implementation broken? Trace to the service/validator. Do not delete or weaken tests to make them pass
- **Build error in test project** → check `GlobalUsings.cs` — missing namespace or cross-layer import

Test files mirror `{Service}/Verendar.{Service}.Application/Services/` and `Validators/` under `{Service}/Verendar.{Service}.Tests/`.
