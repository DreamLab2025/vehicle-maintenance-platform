# Backend Testing — C# / .NET

To apply test pyramid, frameworks, and practices for .NET backends (xUnit, Moq, WebApplicationFactory, Testcontainers). Use when adding or reviewing tests.

---

## Test Pyramid (70–20–10)

```
        /\
       /E2E\     10% — End-to-end / API
      /------\
     /Integr.\ 20% — Integration (DB, HTTP)
    /----------\
   /   Unit     \ 70% — Unit (services, validators)
  /--------------\
```

- **Unit:** Fast, cheap, isolate logic; mock `IUnitOfWork` / repos.
- **Integration:** Real DB (Testcontainers) or in-memory; verify EF, endpoints.
- **E2E/API:** Full app; validate critical flows only (expensive).

---

## Unit Testing (C#)

### Frameworks

| Framework | Use case |
| --------- | -------- |
| **xUnit** | Default for .NET; no shared state, parallel by default |
| **NUnit** | `[SetUp]`/`[TearDown]`, parameterized `[TestCase]` |
| **MSTest** | Legacy / VS integration |

**Common packages:** `xunit`, `Moq`, `FluentAssertions`, `Microsoft.NET.Test.Sdk`.

### Structure (AAA + single responsibility)

```csharp
public class ProjectServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedProject()
    {
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var uow = new Mock<IUnitOfWork>();
        var sut = new ProjectService(repo.Object, uow.Object);

        var result = await sut.CreateAsync(new CreateProjectRequest { Name = "P1" }, default);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("P1");
        repo.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenNameExists_ReturnsFailure()
    {
        var repo = new Mock<IProjectRepository>();
        repo.Setup(r => r.ExistsAsync("P1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sut = new ProjectService(repo.Object, Mock.Of<IUnitOfWork>());

        var result = await sut.CreateAsync(new CreateProjectRequest { Name = "P1" }, default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }
}
```

### Mocking (Moq)

- Mock interfaces only; keep domain entities real where possible.
- Use `It.Is<T>(...)` for complex args; `Verify(..., Times.Once)` for side effects.
- For `IUnitOfWork.SaveChangesAsync`, mock to return `Task.CompletedTask`; no need to track calls unless testing rollback.

---

## Integration Testing

### WebApplicationFactory (API + real or replaced services)

```csharp
public class ProjectApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProjectApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                // Replace DB with test DB or in-memory
                s.RemoveAll<DbContextOptions<ResearchHubDbContext>>();
                s.AddDbContext<ResearchHubDbContext>(o =>
                    o.UseNpgsql(TestDbConnectionString));
            }))
            .CreateClient();
    }

    [Fact]
    public async Task POST_projects_WithValidBody_Returns201()
    {
        var body = new { name = "Test Project" };
        var res = await _client.PostAsJsonAsync("/api/v1/projects", body);
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await res.Content.ReadFromJsonAsync<ApiResponse<ProjectResponse>>();
        json!.Data!.Name.Should().Be("Test Project");
    }
}
```

- Use `WebApplicationFactory<Program>` with `Program` exposed (e.g. `public partial class Program { }`) or a custom entry point.
- Prefer one DB state per test (transaction rollback or clean tables in ctor/fixture).

### Database: Testcontainers (PostgreSQL)

```csharp
public class DbFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync() => await _container.StartAsync();
}
```

- Use same major version as dev (e.g. 17). Run migrations in fixture or per test class.
- Keep container startup in a shared fixture to avoid per-test cost.

---

## Validator Tests

Test FluentValidation validators in isolation (unit).

```csharp
[Theory]
[InlineData(null, false)]
[InlineData("", false)]
[InlineData("valid-name", true)]
public void Name_Validation(string? name, bool isValid)
{
    var validator = new CreateProjectRequestValidator();
    var result = validator.TestValidate(new CreateProjectRequest { Name = name! });
    result.IsValid.Should().Be(isValid);
}
```

Use `FluentValidation.TestHelper` (`TestValidate`) for clear assertion on `Errors`.

---

## Code Coverage

- **Targets:** 80%+ overall; 100% on auth, payment, and data-write paths.
- **Tool:** `coverlet.collector` + report generator; or built-in `dotnet test --collect:"XPlat Code Coverage"`.
- **CI:** Fail on coverage drop or threshold (e.g. `CoverletOutputFormat=opencover` + Sonar/GitHub Action).

---

## Best Practices

1. **AAA:** Arrange (mocks + input), Act (call SUT), Assert (FluentAssertions).
2. **Names:** `Method_Scenario_ExpectedResult` (e.g. `CreateAsync_WhenDuplicateName_Returns409`).
3. **One logical assertion per test**; multiple `Should()` on same object is fine.
4. **No shared mutable state**; xUnit creates new class instance per test.
5. **Fast unit tests** (< tens of ms); integration may use shared container.
6. **Deterministic:** no `Thread.Sleep`; use `TaskCompletionSource` or fake time if needed.
7. **Independent:** tests must not depend on order or global state.

---

## Checklist

- [ ] Unit tests for services (success + validation/duplicate/not-found).
- [ ] Unit tests for validators (valid + invalid cases).
- [ ] Integration tests for critical API endpoints (create/update/delete + auth).
- [ ] DB tests use Testcontainers or dedicated test DB; migrations applied.
- [ ] Coverage collected in CI; threshold or trend enforced.
- [ ] No flaky tests; fix or quarantine with `[Fact(Skip = "reason")]` temporarily.

---

## Commands (this repo)

```bash
dotnet test ResearchHub.sln
dotnet test --filter "FullyQualifiedName~Unit"
dotnet test --filter "FullyQualifiedName~Integration"
```
