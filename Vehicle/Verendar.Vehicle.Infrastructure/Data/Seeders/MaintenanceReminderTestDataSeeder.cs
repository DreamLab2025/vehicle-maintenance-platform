using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Infrastructure.Seeders;

public static class MaintenanceReminderTestDataSeeder
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid WaveAlphaRedVariantId = Guid.Parse("e0000001-0000-0000-0000-000000000001");
    private static readonly Guid TestUserVehicleId = Guid.Parse("f0000001-0000-0000-0000-000000000001");

    private static readonly Guid[] TestReminderIds =
    [
        Guid.Parse("f0000002-0000-0000-0000-000000000002"),
        Guid.Parse("f0000003-0000-0000-0000-000000000003"),
        Guid.Parse("f0000004-0000-0000-0000-000000000004"),
        Guid.Parse("f0000005-0000-0000-0000-000000000005")
    ];

    private static readonly (Guid PartCategoryId, ReminderLevel Level, int LastReplacementOdometer, int PredictedNextOdometer, int KmInterval, int MonthsInterval, decimal PercentageRemaining, int MonthsAgo)[] DeclaredPartReminderConfigs =
    {
        (Guid.Parse("c0000001-0000-0000-0000-000000000001"), ReminderLevel.Critical, 12500, 14500, 2000, 3, 0m, 2),

        (Guid.Parse("c0000002-0000-0000-0000-000000000002"), ReminderLevel.High, 0, 16000, 16000, 24, 6.25m, 10),

        (Guid.Parse("c0000005-0000-0000-0000-000000000005"), ReminderLevel.High, 8000, 16000, 8000, 12, 12.5m, 5),

        (Guid.Parse("c0000004-0000-0000-0000-000000000004"), ReminderLevel.Medium, 11000, 21000, 10000, 12, 60m, 3),
    };

    private const int SeedCurrentOdometer = 15000;

    private static readonly Guid[] UndeclaredPartCategoryIds =
    {
        Guid.Parse("c0000006-0000-0000-0000-000000000006"), // Air Filter
        Guid.Parse("c0000007-0000-0000-0000-000000000007"), // Chain Sprocket
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

        var purchaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-10));

        var userVehicle = new UserVehicle
        {
            Id = TestUserVehicleId,
            UserId = userId,
            VehicleVariantId = WaveAlphaRedVariantId,
            LicensePlate = "59-TEST-01",
            PurchaseDate = purchaseDate,
            CurrentOdometer = SeedCurrentOdometer,
            LastOdometerUpdate = staleDate,
            AverageKmPerDay = 50,
            NeedsOnboarding = false,
            Status = EntityStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.UserVehicles.Add(userVehicle);
        await db.SaveChangesAsync(cancellationToken);

        // Lịch sử Odometer rải đều trong 10 tháng
        var odometerHistories = new[]
        {
            new OdometerHistory
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                OdometerValue = 6000,
                RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-6)),
                KmOnRecordedDate = 6000,
                Source = OdometerSource.ManualInput,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                CreatedBy = TestUserId
            },
            new OdometerHistory
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                OdometerValue = 10500,
                RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                KmOnRecordedDate = 4500,
                Source = OdometerSource.ManualInput,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                CreatedBy = TestUserId
            },
            new OdometerHistory
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                OdometerValue = 13500,
                RecordedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
                KmOnRecordedDate = 3000,
                Source = OdometerSource.ManualInput,
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                CreatedBy = TestUserId
            },
            new OdometerHistory
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                OdometerValue = SeedCurrentOdometer,
                RecordedDate = staleDate,
                KmOnRecordedDate = 1500,
                Source = OdometerSource.ManualInput,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = TestUserId
            }
        };
        db.OdometerHistories.AddRange(odometerHistories);
        await db.SaveChangesAsync(cancellationToken);

        var reminderIndex = 0;
        foreach (var (partCategoryId, level, lastReplacementOdo, predictedNextOdo, kmInterval, monthsInterval, pct, monthsAgo) in DeclaredPartReminderConfigs)
        {
            var lastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-monthsAgo));
            var predictedNextDate = lastReplacementDate.AddMonths(monthsInterval);

            var partTracking = new PartTracking
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                PartCategoryId = partCategoryId,
                LastReplacementOdometer = lastReplacementOdo,
                LastReplacementDate = lastReplacementDate,
                PredictedNextOdometer = predictedNextOdo,
                PredictedNextDate = predictedNextDate,
                CustomKmInterval = kmInterval,
                CustomMonthsInterval = monthsInterval,
                IsDeclared = true,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-monthsAgo),
                CreatedBy = TestUserId
            };
            db.VehiclePartTrackings.Add(partTracking);
            await db.SaveChangesAsync(cancellationToken);

            var reminder = new MaintenanceReminder
            {
                Id = TestReminderIds[reminderIndex++],
                VehiclePartTrackingId = partTracking.Id,
                CurrentOdometer = SeedCurrentOdometer,
                TargetOdometer = predictedNextOdo,
                TargetDate = predictedNextDate,
                Level = level,
                PercentageRemaining = pct,
                IsNotified = level == ReminderLevel.Critical, // Đánh dấu đã báo nếu quá hạn
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.MaintenanceReminders.Add(reminder);
        }
        await db.SaveChangesAsync(cancellationToken);

        foreach (var partCategoryId in UndeclaredPartCategoryIds)
        {
            var partTracking = new PartTracking
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
            "Seeded test user vehicle (UserId: {UserId}, UserVehicleId: {UserVehicleId}) with realistic data: 4 declared parts, 2 undeclared parts.",
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

        var reminderIndex = 0;
        var addedCount = 0;
        foreach (var (partCategoryId, level, lastReplacementOdo, predictedNextOdo, kmInterval, monthsInterval, pct, monthsAgo) in DeclaredPartReminderConfigs)
        {
            var existingReminder = await db.MaintenanceReminders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == TestReminderIds[reminderIndex], cancellationToken);

            if (existingReminder != null)
            {
                reminderIndex++;
                continue;
            }

            var partTracking = await db.VehiclePartTrackings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pt => pt.UserVehicleId == userVehicleId && pt.PartCategoryId == partCategoryId, cancellationToken);

            if (partTracking == null)
            {
                var lastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-monthsAgo));
                var predictedNextDate = lastReplacementDate.AddMonths(monthsInterval);

                partTracking = new PartTracking
                {
                    Id = Guid.CreateVersion7(),
                    UserVehicleId = userVehicleId,
                    PartCategoryId = partCategoryId,
                    LastReplacementOdometer = lastReplacementOdo,
                    LastReplacementDate = lastReplacementDate,
                    PredictedNextOdometer = predictedNextOdo,
                    PredictedNextDate = predictedNextDate,
                    CustomKmInterval = kmInterval,
                    CustomMonthsInterval = monthsInterval,
                    IsDeclared = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddMonths(-monthsAgo),
                    CreatedBy = TestUserId
                };
                db.VehiclePartTrackings.Add(partTracking);
                await db.SaveChangesAsync(cancellationToken);
            }

            var existingRemindersForTracking = await db.MaintenanceReminders
                .IgnoreQueryFilters()
                .Where(r => r.VehiclePartTrackingId == partTracking.Id)
                .ToListAsync(cancellationToken);

            foreach (var r in existingRemindersForTracking)
            {
                if (r.IsCurrent)
                {
                    r.IsCurrent = false;
                    db.MaintenanceReminders.Update(r);
                }
            }
            await db.SaveChangesAsync(cancellationToken);

            var reminder = new MaintenanceReminder
            {
                Id = TestReminderIds[reminderIndex++],
                VehiclePartTrackingId = partTracking.Id,
                CurrentOdometer = SeedCurrentOdometer,
                TargetOdometer = predictedNextOdo,
                TargetDate = partTracking.PredictedNextDate,
                Level = level,
                PercentageRemaining = pct,
                IsNotified = level == ReminderLevel.Critical,
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.MaintenanceReminders.Add(reminder);
            await db.SaveChangesAsync(cancellationToken);
            existingLevels.Add(level);
            addedCount++;
            logger?.LogInformation("Added MaintenanceReminder level {Level} for test vehicle {UserVehicleId}", level, userVehicleId);
        }

        if (addedCount > 0)
        {
            logger?.LogInformation("Added {Count} maintenance reminders for test vehicle {UserVehicleId}", addedCount, userVehicleId);
        }

        foreach (var partCategoryId in UndeclaredPartCategoryIds)
        {
            var exists = await db.VehiclePartTrackings
                .IgnoreQueryFilters()
                .AnyAsync(pt => pt.UserVehicleId == userVehicleId && pt.PartCategoryId == partCategoryId, cancellationToken);
            if (exists)
                continue;

            var partTracking = new PartTracking
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