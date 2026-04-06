using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Verendar.Identity.Application.Shared.Helpers;

namespace Verendar.Identity.Infrastructure.Data.Seeders;

public static class CsvUserSeeder
{
    private const string DefaultPassword = "12345@Abc";
    private const string ResourceName =
        "Verendar.Identity.Infrastructure.Data.Seeders.seed-users.csv";

    public static async Task SeedAsync(
        UserDbContext db,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var csvRows = ReadCsvRows();
        if (csvRows.Count == 0)
        {
            logger?.LogWarning("CsvUserSeeder: no rows found in embedded CSV");
            return;
        }

        // Collect all normalised emails from CSV to do a single existence check
        var normalizedEmails = csvRows
            .Select(r => EmailHelper.Normalize(r.Email))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingEmails = await db.Users
            .IgnoreQueryFilters()
            .Where(u => normalizedEmails.Contains(u.Email))
            .Select(u => u.Email)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var hasher = new PasswordHasher<User>();
        var toInsert = new List<(User User, DateTime DesiredCreatedAtUtc)>();

        foreach (var row in csvRows)
        {
            var email = EmailHelper.Normalize(row.Email);
            if (existingEmails.Contains(email))
                continue;

            var desiredCreatedAtUtc = EnsureUtc(row.CreatedAt);

            var id = Guid.CreateVersion7();
            var user = new User
            {
                Id = id,
                Email = email,
                FullName = row.FullName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(row.Phone) ? null : row.Phone.Trim(),
                PasswordHash = string.Empty,
                EmailVerified = true,
                PhoneNumberVerified = false,
                Roles = [UserRole.User],
                // BaseDbContext will overwrite this to UtcNow — we fix it below
                CreatedAt = desiredCreatedAtUtc,
                CreatedBy = id,
            };
            user.PasswordHash = hasher.HashPassword(user, DefaultPassword);
            toInsert.Add((user, desiredCreatedAtUtc));
        }

        if (toInsert.Count == 0)
        {
            logger?.LogDebug("CsvUserSeeder: all {Count} CSV users already exist — skipped", csvRows.Count);
            return;
        }

        // Step 1: bulk insert (BaseDbContext will stamp CreatedAt = UtcNow)
        db.Users.AddRange(toInsert.Select(t => t.User));
        await db.SaveChangesAsync(cancellationToken);

        // Step 2: fix CreatedAt to the values from CSV via raw SQL
        //         (EF table name is "Users" — default convention for DbSet<User>)
        foreach (var (user, desiredCreatedAtUtc) in toInsert)
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
            $"""UPDATE "Users" SET "CreatedAt" = {desiredCreatedAtUtc} WHERE "Id" = {user.Id}""",
                cancellationToken);
        }

        logger?.LogWarning(
            "CsvUserSeeder: inserted {Count} users (skipped {Skipped} existing)",
            toInsert.Count,
            csvRows.Count - toInsert.Count);
    }

    private static List<CsvRow> ReadCsvRows()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
            return [];

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<CsvRow>().ToList();
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    private sealed class CsvRow
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
