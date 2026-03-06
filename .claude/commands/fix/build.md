Fix all build and compilation errors in the Verendar solution.

Run `dotnet build Verendar.sln` and analyze the output. For each error:

1. Read the file at the error location
2. Understand why the error occurred (type mismatch, missing using, changed interface, etc.)
3. Fix with the minimal correct change — do not restructure code to avoid the error

Common causes in this project:
- Interface in Domain changed but Infrastructure implementation not updated
- DTO shape changed but mapping extension method not updated (`ToResponse()`, `ToEntity()`)
- Missing `async`/`await` on a method that returns `Task`
- Nullable reference type not handled (`?` annotation or null check missing)
- New required constructor parameter not passed in factory/repository

After fixing, run `dotnet build Verendar.sln` again to confirm clean build.
