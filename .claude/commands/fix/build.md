Fix all build and compilation errors in the Verendar solution.

Run `dotnet build Verendar.sln` from the repo root and analyze output. For each error:

1. Read the file at the error location
2. Understand why it occurred (type mismatch, missing using, changed interface, etc.)
3. Fix with the minimal correct change — do not restructure code

Common causes in this project:
- Interface in Domain changed but Infrastructure implementation not updated
- DTO shape changed but `ToResponse()` / `ToEntity()` extension not updated
- Missing `async`/`await` on `Task`-returning method
- Nullable reference not handled
- New required constructor parameter not passed
- EF Core configuration not matching entity changes
- MassTransit consumer interface mismatch

After fixing, run `dotnet build Verendar.sln` again to confirm clean build.
