# Code Quality — SOLID, Design Patterns, Clean Code

To apply SOLID principles, common design patterns, clean code practices, and refactoring techniques in this .NET Clean Architecture backend.

---

## SOLID

### Single Responsibility (SRP) — one class, one reason to change
```csharp
// ❌ One service doing too much
class ProjectService { public Task CreateAsync(req) { /* save + email + upload */ } }

// ✅ Each class has one job
class ProjectService    { public Task CreateAsync(req) { /* business logic only */ } }
class EmailService      { public Task SendAsync(string to, string subject) { } }
class FileUploadService { public Task<string> UploadAsync(IFormFile file)  { } }
```

### Open/Closed (OCP) — open for extension, closed for modification
```csharp
// ❌ Modify class to add formats
class ReportExporter {
    public byte[] Export(string type, IEnumerable<Project> data) {
        if (type == "pdf") { /* ... */ }
        else if (type == "csv") { /* ... */ } // adding Excel = modify here
    }
}

// ✅ Strategy pattern — add formats without changing ReportExporter
interface IExportStrategy { byte[] Export(IEnumerable<Project> data); }
class PdfExportStrategy   : IExportStrategy { /* ... */ }
class CsvExportStrategy   : IExportStrategy { /* ... */ }
class ReportExporter(IExportStrategy strategy) {
    public byte[] Export(IEnumerable<Project> data) => strategy.Export(data);
}
```

### Liskov Substitution (LSP) — subtypes must be substitutable, never override to throw
```csharp
// ❌ Violates LSP
class ReadOnlyProjectRepository : IProjectRepository {
    public Task AddAsync(Project p) => throw new NotSupportedException();
}

// ✅ Split interfaces so implementations never need to throw
interface IReadProjectRepository  { Task<Project?> GetByIdAsync(Guid id, CancellationToken ct); }
interface IWriteProjectRepository { Task AddAsync(Project p); }
```

### Interface Segregation (ISP) — clients shouldn't depend on interfaces they don't use
```csharp
// ❌ One fat interface forces every repo to implement BulkImport
interface IRepository<T> { T GetById(Guid id); void Save(T e); void BulkImport(IEnumerable<T> items); }

// ✅ Small, focused interfaces
interface IReadRepository<T>  { Task<T?> GetByIdAsync(Guid id, CancellationToken ct); }
interface IWriteRepository<T> { Task AddAsync(T entity); void Remove(T entity); }
interface IBulkRepository<T>  { Task BulkImportAsync(IEnumerable<T> items, CancellationToken ct); }
```

### Dependency Inversion (DIP) — depend on abstractions, not concretions
```csharp
// ❌ Tight coupling — hard to test
class ProjectService { private readonly EfProjectRepository _repo = new(); }

// ✅ Constructor injection with interfaces
class ProjectService(IProjectRepository repo, IEmailService email, IUnitOfWork uow) {
    public async Task<ApiResponse<ProjectResponse>> CreateAsync(CreateProjectRequest req, CancellationToken ct) {
        var project = req.ToEntity();
        await repo.AddAsync(project);
        await uow.SaveChangesAsync(ct);
        return ApiResponse<ProjectResponse>.SuccessResponse(project.ToResponse(), 201);
    }
}
```

---

## Design Patterns

### Repository + Unit of Work
Abstraction between business logic and data access. Transactions span multiple repos via UoW.

```csharp
interface IProjectRepository : IGenericRepository<Project> {
    Task<Project?> GetWithMembersAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(string name, CancellationToken ct = default);
}

class ProjectService(IProjectRepository repo, IUnitOfWork uow) {
    public async Task<ApiResponse<ProjectResponse>> CreateAsync(CreateProjectRequest req, CancellationToken ct) {
        if (await repo.ExistsAsync(req.Name, ct))
            return ApiResponse<ProjectResponse>.FailureResponse(AppMessages.ProjectNameExists, 409);
        var project = req.ToEntity();
        await repo.AddAsync(project);
        await uow.SaveChangesAsync(ct);
        return ApiResponse<ProjectResponse>.SuccessResponse(project.ToResponse(), 201);
    }
}
```

### Factory
Create objects without specifying the exact class. Use when the type varies at runtime.

```csharp
interface INotificationChannel { Task SendAsync(string to, string msg, CancellationToken ct); }
class EmailChannel : INotificationChannel { /* ... */ }
class SmsChannel   : INotificationChannel { /* ... */ }

class NotificationChannelFactory {
    public static INotificationChannel Create(string type) => type switch {
        "email" => new EmailChannel(),
        "sms"   => new SmsChannel(),
        _       => throw new ArgumentException($"Unknown channel: {type}")
    };
}
```

### Decorator
Add behavior without modifying the original class. Useful for cross-cutting concerns (logging, caching, retry).

```csharp
interface IFileStorageService { Task<string> UploadAsync(Stream file, string name, CancellationToken ct); }
class S3StorageService : IFileStorageService { /* actual upload */ }

class LoggingStorageDecorator(IFileStorageService inner, ILogger logger) : IFileStorageService {
    public async Task<string> UploadAsync(Stream file, string name, CancellationToken ct) {
        logger.LogInformation("Uploading: {Name}", name);
        var url = await inner.UploadAsync(file, name, ct);
        logger.LogInformation("Uploaded: {Url}", url);
        return url;
    }
}
```

---

## Clean Code

### Meaningful names — no abbreviations, no magic numbers
```csharp
// ❌
async Task<object> Proc(Guid id, int t) { if (file.Length > 52428800) { } }

// ✅
const long MaxFileSizeBytes = 50 * 1024 * 1024;
async Task<ApiResponse<ProjectResponse>> GetProjectWithMembersAsync(Guid projectId, CancellationToken ct) {
    if (file.Length > MaxFileSizeBytes) { }
}
```

### Guard clauses at top — no deep nesting
```csharp
// ❌ Arrow anti-pattern
async Task CreateAsync(req) {
    if (req != null) { if (await repo.ExistsAsync(req.Name)) { /* ... */ } }
}

// ✅
async Task CreateAsync(req, ct) {
    if (req is null) return ApiResponse<ProjectResponse>.FailureResponse(AppMessages.InvalidRequest, 400);
    if (await repo.ExistsAsync(req.Name, ct))
        return ApiResponse<ProjectResponse>.FailureResponse(AppMessages.ProjectNameExists, 409);
    // happy path
}
```

### Error handling — use AppMessages, let infrastructure exceptions propagate
```csharp
// ❌ Silent catch / raw strings
try { var user = await repo.GetByIdAsync(id); return user; }
catch (Exception e) { Console.WriteLine(e); return null; }

// ✅
var user = await repo.GetByIdAsync(id, ct);
if (user is null)
    return ApiResponse<UserResponse>.FailureResponse(AppMessages.UserNotFound, 404);
// DB/network exceptions propagate to GlobalExceptionsMiddleware → 500
```

### DRY — one validator, shared across create/update
```csharp
// ❌ Duplicated validation in every endpoint handler
if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 200) return Error(...);

// ✅ One validator used via ValidateRequest()
class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest> {
    public CreateProjectRequestValidator() {
        RuleFor(x => x.Name).NotEmpty().WithValidatorMessage(ValidatorMessages.ProjectNameRequired)
                             .MaximumLength(255).WithValidatorMessage(ValidatorMessages.ProjectNameMaxLength);
    }
}
```

---

## Refactoring

### Extract Method — pull single-purpose blocks into named methods
```csharp
// ❌ 150-line method mixing validation, business rules, persistence, email
async Task ProcessApplicationAsync(Guid projectId, Guid applicantId) { /* ... */ }

// ✅
async Task ProcessApplicationAsync(Guid projectId, Guid applicantId, CancellationToken ct) {
    var (project, applicant) = await ValidateAndGetAsync(projectId, applicantId, ct);
    await AssertApplicationQuotaAsync(project, ct);
    var application = await SaveApplicationAsync(project, applicant, ct);
    await NotifyOwnerAsync(project, applicant, ct);
}
```

### Replace Conditional with Strategy — swap type-checking chains with interfaces
```csharp
// ❌ Type switch — every new type = modify this method
async Task SendNotificationAsync(string type, string to, string msg) {
    if (type == "email") { /* ... */ }
    else if (type == "sms") { /* ... */ }
}

// ✅ Strategy — add channels without touching the service
interface INotificationChannel { bool CanHandle(string type); Task SendAsync(string to, string msg, CancellationToken ct); }
class NotificationService(IEnumerable<INotificationChannel> channels) {
    public Task SendAsync(string type, string to, string msg, CancellationToken ct) {
        var channel = channels.First(c => c.CanHandle(type));
        return channel.SendAsync(to, msg, ct);
    }
}
```

---

## Checklist

```
[ ] SOLID principles applied — each class has one reason to change
[ ] Methods are small and focused (<20 lines ideal)
[ ] Meaningful names — no abbreviations, no magic numbers
[ ] Guard clauses at top — no deep nesting (arrow anti-pattern)
[ ] AppMessages used for all FailureResponse — no raw strings
[ ] ValidatorMessages used for all validator rules — no bare .WithErrorCode + .WithMessage
[ ] No silent catch blocks — infrastructure exceptions propagate to middleware
[ ] DRY — validation, mapping, and query logic not duplicated
[ ] All dependencies injected via interfaces — testable
[ ] Comments explain "why", not "what"
```
