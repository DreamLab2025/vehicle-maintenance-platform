using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Seeders;

public static class MaintenanceReminderTestDataSeeder
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid WaveAlphaRedVariantId = Guid.Parse("e0000001-0000-0000-0000-000000000001");
    private static readonly (Guid PartCategoryId, ReminderLevel Level, int CurrentOdometer, int TargetOdometer, decimal PercentageRemaining)[] DeclaredPartReminderConfigs =
    {
        (Guid.Parse("c0000001-0000-0000-0000-000000000001"), ReminderLevel.Critical, 5000, 6000, 5m),   // Dầu nhớt
        (Guid.Parse("c0000002-0000-0000-0000-000000000002"), ReminderLevel.High, 15000, 20000, 25m), // Lốp xe
        (Guid.Parse("c0000004-0000-0000-0000-000000000004"), ReminderLevel.Medium, 7000, 10000, 30m), // Má phanh
    };

    private static readonly Guid[] UndeclaredPartCategoryIds =
    {
        Guid.Parse("c0000006-0000-0000-0000-000000000006"),
        Guid.Parse("c0000005-0000-0000-0000-000000000005"),
    };

    public static async Task SeedAsync(VehicleDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var userId = TestUserId;
        var existingVehicle = await db.UserVehicles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

        if (existingVehicle != null)
        {
            await EnsureAllReminderLevelsForVehicleAsync(db, existingVehicle.Id, logger, cancellationToken);
            logger?.LogDebug("Test user vehicle already exists for user {UserId}, reminders ensured", userId);
            return;
        }

        var staleDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5);
        var userVehicle = new UserVehicle
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            VehicleVariantId = WaveAlphaRedVariantId,
            LicensePlate = "59-TEST-01",
            CurrentOdometer = 5000,
            LastOdometerUpdate = staleDate,
            NeedsOnboarding = false,
            Status = EntityStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.UserVehicles.Add(userVehicle);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var (partCategoryId, level, currentOdo, targetOdo, pct) in DeclaredPartReminderConfigs)
        {
            var partTracking = new VehiclePartTracking
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                PartCategoryId = partCategoryId,
                LastReplacementOdometer = currentOdo - 3000,
                LastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
                IsDeclared = true,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.VehiclePartTrackings.Add(partTracking);
            await db.SaveChangesAsync(cancellationToken);

            var reminder = new MaintenanceReminder
            {
                Id = Guid.CreateVersion7(),
                VehiclePartTrackingId = partTracking.Id,
                CurrentOdometer = currentOdo,
                TargetOdometer = targetOdo,
                Level = level,
                PercentageRemaining = pct,
                IsNotified = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.MaintenanceReminders.Add(reminder);
        }
        await db.SaveChangesAsync(cancellationToken);

        foreach (var partCategoryId in UndeclaredPartCategoryIds)
        {
            var partTracking = new VehiclePartTracking
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                PartCategoryId = partCategoryId,
                IsDeclared = false,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.VehiclePartTrackings.Add(partTracking);
        }
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Seeded test user vehicle (UserId: {UserId}, UserVehicleId: {UserVehicleId}) with 3 declared parts (IsDeclared=true, Critical/High/Medium reminders), 2 undeclared parts (IsDeclared=false); stale odometer for OdometerReminderJob",
            userId, userVehicle.Id);
    }

    private static async Task EnsureAllReminderLevelsForVehicleAsync(
        VehicleDbContext db,
        Guid userVehicleId,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        var existingLevels = await db.MaintenanceReminders
            .IgnoreQueryFilters()
            .Include(m => m.PartTracking)
            .Where(m => m.PartTracking!.UserVehicleId == userVehicleId)
            .Select(m => m.Level)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var (partCategoryId, level, currentOdo, targetOdo, pct) in DeclaredPartReminderConfigs)
        {
            if (existingLevels.Contains(level))
                continue;

            var partTracking = await db.VehiclePartTrackings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pt => pt.UserVehicleId == userVehicleId && pt.PartCategoryId == partCategoryId, cancellationToken);

            if (partTracking == null)
            {
                partTracking = new VehiclePartTracking
                {
                    Id = Guid.CreateVersion7(),
                    UserVehicleId = userVehicleId,
                    PartCategoryId = partCategoryId,
                    LastReplacementOdometer = currentOdo - 3000,
                    LastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
                    IsDeclared = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = TestUserId
                };
                db.VehiclePartTrackings.Add(partTracking);
                await db.SaveChangesAsync(cancellationToken);
            }

            var reminder = new MaintenanceReminder
            {
                Id = Guid.CreateVersion7(),
                VehiclePartTrackingId = partTracking.Id,
                CurrentOdometer = currentOdo,
                TargetOdometer = targetOdo,
                Level = level,
                PercentageRemaining = pct,
                IsNotified = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.MaintenanceReminders.Add(reminder);
            await db.SaveChangesAsync(cancellationToken);
            existingLevels.Add(level);
            logger?.LogInformation("Added MaintenanceReminder level {Level} for test vehicle {UserVehicleId}", level, userVehicleId);
        }

        foreach (var partCategoryId in UndeclaredPartCategoryIds)
        {
            var exists = await db.VehiclePartTrackings
                .IgnoreQueryFilters()
                .AnyAsync(pt => pt.UserVehicleId == userVehicleId && pt.PartCategoryId == partCategoryId, cancellationToken);
            if (exists)
                continue;
            var partTracking = new VehiclePartTracking
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicleId,
                PartCategoryId = partCategoryId,
                IsDeclared = false,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.VehiclePartTrackings.Add(partTracking);
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Added undeclared part tracking (IsDeclared=false) for test vehicle {UserVehicleId}, PartCategoryId {PartCategoryId}", userVehicleId, partCategoryId);
        }
    }
}
