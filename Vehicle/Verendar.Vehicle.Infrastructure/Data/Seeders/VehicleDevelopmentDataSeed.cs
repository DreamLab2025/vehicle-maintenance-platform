using Verendar.Vehicle.Domain.Enums;

namespace Verendar.Vehicle.Infrastructure.Seeders;

public static class VehicleDevelopmentDataSeed
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
        (Guid.Parse("c0000001-0000-0000-0000-000000000001"), ReminderLevel.Critical, 12500, 14500, 2000, 3, 2m, 2),
        (Guid.Parse("c0000002-0000-0000-0000-000000000002"), ReminderLevel.Critical, 12000, 15000, 3000, 6, 4m, 3),
        (Guid.Parse("c0000005-0000-0000-0000-000000000005"), ReminderLevel.High, 0, 16000, 16000, 24, 10m, 10),
        (Guid.Parse("c0000004-0000-0000-0000-000000000004"), ReminderLevel.Medium, 11000, 21000, 10000, 12, 20m, 3),
    };

    private const int SeedCurrentOdometer = 15000;

    private static readonly Guid[] UndeclaredPartCategoryIds =
    {
        Guid.Parse("c0000006-0000-0000-0000-000000000006"),
        Guid.Parse("c0000007-0000-0000-0000-000000000007"),
    };

    private static readonly Guid DemoMaintenanceRecordId1 = Guid.Parse("f00000a1-0000-0000-0000-000000000001");
    private static readonly Guid DemoMaintenanceRecordId2 = Guid.Parse("f00000a2-0000-0000-0000-000000000002");
    private static readonly Guid DemoMaintenanceRecordId3 = Guid.Parse("f00000a3-0000-0000-0000-000000000003");
    private static readonly Guid DemoMaintenanceRecordId4 = Guid.Parse("f00000a4-0000-0000-0000-000000000004");

    private static readonly Guid DemoMaintenanceItemId1 = Guid.Parse("f00000b1-0000-0000-0000-000000000001");
    private static readonly Guid DemoMaintenanceItemId2 = Guid.Parse("f00000b2-0000-0000-0000-000000000002");
    private static readonly Guid DemoMaintenanceItemId3 = Guid.Parse("f00000b3-0000-0000-0000-000000000003");
    private static readonly Guid DemoMaintenanceItemId4 = Guid.Parse("f00000b4-0000-0000-0000-000000000004");

    private static void ApplyDeclaredPartTrackingSeed(
        PartTracking partTracking,
        int lastReplacementOdo,
        DateOnly lastReplacementDate,
        int predictedNextOdo,
        DateOnly predictedNextDate,
        int kmInterval,
        int monthsInterval)
    {
        partTracking.LastReplacementOdometer = lastReplacementOdo;
        partTracking.LastReplacementDate = lastReplacementDate;
        partTracking.PredictedNextOdometer = predictedNextOdo;
        partTracking.PredictedNextDate = predictedNextDate;
        partTracking.CustomKmInterval = kmInterval;
        partTracking.CustomMonthsInterval = monthsInterval;
        partTracking.IsDeclared = true;
    }

    private static async Task EnsureDemoMaintenanceRecordsAsync(
        VehicleDbContext db,
        Guid userVehicleId,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        if (await db.MaintenanceRecords
                .IgnoreQueryFilters()
                .AnyAsync(r => r.Id == DemoMaintenanceRecordId1, cancellationToken))
            return;

        static DateTime createdAtUtc(DateOnly serviceDate) =>
            serviceDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // Aligns with DeclaredPartReminderConfigs: monthsAgo 2 / c0000001 @ 12500
        var serviceDate1 = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var record1 = new MaintenanceRecord
        {
            Id = DemoMaintenanceRecordId1,
            UserVehicleId = userVehicleId,
            ServiceDate = serviceDate1,
            OdometerAtService = 12500,
            GarageName = "Garage demo — Quận 1",
            TotalCost = 520_000m,
            Notes = "Thay nhớt + lọc gió (seed demo).",
            CreatedAt = createdAtUtc(serviceDate1),
            CreatedBy = TestUserId
        };
        db.MaintenanceRecords.Add(record1);
        await db.SaveChangesAsync(cancellationToken);
        db.MaintenanceRecordItems.Add(new MaintenanceRecordItem
        {
            Id = DemoMaintenanceItemId1,
            MaintenanceRecordId = record1.Id,
            PartCategoryId = Guid.Parse("c0000001-0000-0000-0000-000000000001"),
            Price = 520_000m,
            Notes = "Nhớt + lọc",
            UpdatesTracking = true,
            CreatedAt = createdAtUtc(serviceDate1),
            CreatedBy = TestUserId
        });

        // monthsAgo 3 / c0000002 @ 12000
        var serviceDate2 = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3).AddDays(3));
        var record2 = new MaintenanceRecord
        {
            Id = DemoMaintenanceRecordId2,
            UserVehicleId = userVehicleId,
            ServiceDate = serviceDate2,
            OdometerAtService = 12000,
            GarageName = "Garage demo — Thủ Đức",
            TotalCost = 380_000m,
            Notes = "Thay dây curoa (seed demo).",
            CreatedAt = createdAtUtc(serviceDate2),
            CreatedBy = TestUserId
        };
        db.MaintenanceRecords.Add(record2);
        await db.SaveChangesAsync(cancellationToken);
        db.MaintenanceRecordItems.Add(new MaintenanceRecordItem
        {
            Id = DemoMaintenanceItemId2,
            MaintenanceRecordId = record2.Id,
            PartCategoryId = Guid.Parse("c0000002-0000-0000-0000-000000000002"),
            Price = 380_000m,
            UpdatesTracking = true,
            CreatedAt = createdAtUtc(serviceDate2),
            CreatedBy = TestUserId
        });

        // monthsAgo 3 / c0000004 @ 11000
        var serviceDate3 = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3).AddDays(-5));
        var record3 = new MaintenanceRecord
        {
            Id = DemoMaintenanceRecordId3,
            UserVehicleId = userVehicleId,
            ServiceDate = serviceDate3,
            OdometerAtService = 11000,
            GarageName = "Garage demo — Thủ Đức",
            TotalCost = 210_000m,
            Notes = "Thay phanh / kiểm tra (seed demo).",
            CreatedAt = createdAtUtc(serviceDate3),
            CreatedBy = TestUserId
        };
        db.MaintenanceRecords.Add(record3);
        await db.SaveChangesAsync(cancellationToken);
        db.MaintenanceRecordItems.Add(new MaintenanceRecordItem
        {
            Id = DemoMaintenanceItemId3,
            MaintenanceRecordId = record3.Id,
            PartCategoryId = Guid.Parse("c0000004-0000-0000-0000-000000000004"),
            Price = 210_000m,
            UpdatesTracking = true,
            CreatedAt = createdAtUtc(serviceDate3),
            CreatedBy = TestUserId
        });

        var serviceDate4 = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-10));
        var record4 = new MaintenanceRecord
        {
            Id = DemoMaintenanceRecordId4,
            UserVehicleId = userVehicleId,
            ServiceDate = serviceDate4,
            OdometerAtService = 0,
            GarageName = "Garage demo — mốc đầu",
            TotalCost = 150_000m,
            Notes = "Khai báo / dịch vụ mốc ban đầu (seed demo, km đồng bộ tracking).",
            CreatedAt = createdAtUtc(serviceDate4),
            CreatedBy = TestUserId
        };
        db.MaintenanceRecords.Add(record4);
        await db.SaveChangesAsync(cancellationToken);
        db.MaintenanceRecordItems.Add(new MaintenanceRecordItem
        {
            Id = DemoMaintenanceItemId4,
            MaintenanceRecordId = record4.Id,
            PartCategoryId = Guid.Parse("c0000005-0000-0000-0000-000000000005"),
            Price = 150_000m,
            Notes = "Dịch vụ tổng hợp",
            UpdatesTracking = true,
            CreatedAt = createdAtUtc(serviceDate4),
            CreatedBy = TestUserId
        });

        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation(
            "Seeded demo maintenance history ({Count} records) for UserVehicleId {UserVehicleId}",
            4,
            userVehicleId);
    }

    public static async Task SeedAsync(VehicleDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var userId = TestUserId;
        var existingVehicle = await db.UserVehicles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

        if (existingVehicle != null)
        {
            await EnsureAllReminderLevelsForVehicleAsync(db, existingVehicle.Id, logger, cancellationToken);
            await EnsureDemoMaintenanceRecordsAsync(db, existingVehicle.Id, logger, cancellationToken);
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
            VIN = "DEM00000000000001",
            PurchaseDate = purchaseDate,
            CurrentOdometer = SeedCurrentOdometer,
            LastOdometerUpdate = staleDate,
            AverageKmPerDay = 50,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.UserVehicles.Add(userVehicle);
        await db.SaveChangesAsync(cancellationToken);

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

        for (var i = 0; i < DeclaredPartReminderConfigs.Length; i++)
        {
            var (partCategoryId, level, lastReplacementOdo, predictedNextOdo, kmInterval, monthsInterval, pct, monthsAgo) =
                DeclaredPartReminderConfigs[i];
            var lastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-monthsAgo));
            var predictedNextDate = lastReplacementDate.AddMonths(monthsInterval);

            var partTracking = new PartTracking
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicle.Id,
                PartCategoryId = partCategoryId,
                CreatedAt = DateTime.UtcNow.AddMonths(-monthsAgo),
                CreatedBy = TestUserId
            };
            ApplyDeclaredPartTrackingSeed(
                partTracking,
                lastReplacementOdo,
                lastReplacementDate,
                predictedNextOdo,
                predictedNextDate,
                kmInterval,
                monthsInterval);
            db.VehiclePartTrackings.Add(partTracking);
            await db.SaveChangesAsync(cancellationToken);

            var cycle = new TrackingCycle
            {
                Id = Guid.CreateVersion7(),
                PartTrackingId = partTracking.Id,
                StartOdometer = lastReplacementOdo,
                StartDate = lastReplacementDate,
                TargetOdometer = predictedNextOdo,
                TargetDate = predictedNextDate,
                Status = CycleStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-monthsAgo),
                CreatedBy = TestUserId
            };
            db.TrackingCycles.Add(cycle);
            await db.SaveChangesAsync(cancellationToken);

            var reminder = new MaintenanceReminder
            {
                Id = TestReminderIds[i],
                TrackingCycleId = cycle.Id,
                CurrentOdometer = SeedCurrentOdometer,
                TargetOdometer = predictedNextOdo,
                TargetDate = predictedNextDate,
                Level = level,
                PercentageRemaining = pct,
                IsNotified = false,
                NotifiedDate = null,
                Status = ReminderStatus.Active,
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

                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.VehiclePartTrackings.Add(partTracking);
        }
        await db.SaveChangesAsync(cancellationToken);

        await EnsureDemoMaintenanceRecordsAsync(db, userVehicle.Id, logger, cancellationToken);

        logger?.LogInformation(
            "Seeded test user vehicle (UserId: {UserId}, UserVehicleId: {UserVehicleId}): UserVehicle+VIN, odometer history, 4 declared PartTrackings with active TrackingCycles + reminders (Critical×2, High, Medium), 2 undeclared trackings, 4 MaintenanceRecords — aligned with thin notification grouping.",
            userId, userVehicle.Id);
    }

    private static async Task EnsureAllReminderLevelsForVehicleAsync(
        VehicleDbContext db,
        Guid userVehicleId,
        ILogger? logger,
        CancellationToken cancellationToken)
    {
        var vehicleRow = await db.UserVehicles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == userVehicleId, cancellationToken);
        var reminderCurrentOdo = vehicleRow?.CurrentOdometer ?? SeedCurrentOdometer;

        var addedOrRepaired = 0;
        for (var i = 0; i < DeclaredPartReminderConfigs.Length; i++)
        {
            var (partCategoryId, level, lastReplacementOdo, predictedNextOdo, kmInterval, monthsInterval, pct, monthsAgo) =
                DeclaredPartReminderConfigs[i];
            var expectedReminderId = TestReminderIds[i];
            var lastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-monthsAgo));
            var predictedNextDate = lastReplacementDate.AddMonths(monthsInterval);

            var partTracking = await db.VehiclePartTrackings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pt => pt.UserVehicleId == userVehicleId && pt.PartCategoryId == partCategoryId, cancellationToken);

            if (partTracking == null)
            {
                partTracking = new PartTracking
                {
                    Id = Guid.CreateVersion7(),
                    UserVehicleId = userVehicleId,
                    PartCategoryId = partCategoryId,
                    CreatedAt = DateTime.UtcNow.AddMonths(-monthsAgo),
                    CreatedBy = TestUserId
                };
                db.VehiclePartTrackings.Add(partTracking);
            }

            ApplyDeclaredPartTrackingSeed(
                partTracking,
                lastReplacementOdo,
                lastReplacementDate,
                predictedNextOdo,
                predictedNextDate,
                kmInterval,
                monthsInterval);
            await db.SaveChangesAsync(cancellationToken);

            var existingReminder = await db.MaintenanceReminders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == expectedReminderId, cancellationToken);

            TrackingCycle? linkedCycle = null;
            if (existingReminder != null)
            {
                linkedCycle = await db.TrackingCycles
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == existingReminder.TrackingCycleId, cancellationToken);
            }

            var chainOk = linkedCycle != null
                && linkedCycle.Status == CycleStatus.Active
                && linkedCycle.PartTrackingId == partTracking.Id;

            if (chainOk)
            {
                // PartTracking already aligned above; do not overwrite reminder rows when cycle link is valid.
                await db.SaveChangesAsync(cancellationToken);
                continue;
            }

            var existingCycles = await db.TrackingCycles
                .IgnoreQueryFilters()
                .Include(c => c.Reminders)
                .Where(c => c.PartTrackingId == partTracking.Id && c.Status == CycleStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var c in existingCycles)
            {
                c.Status = CycleStatus.Completed;
                foreach (var r in c.Reminders)
                    r.Status = ReminderStatus.Resolved;
            }
            await db.SaveChangesAsync(cancellationToken);

            var cycle = new TrackingCycle
            {
                Id = Guid.CreateVersion7(),
                PartTrackingId = partTracking.Id,
                StartOdometer = lastReplacementOdo,
                StartDate = lastReplacementDate,
                TargetOdometer = predictedNextOdo,
                TargetDate = predictedNextDate,
                Status = CycleStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMonths(-monthsAgo),
                CreatedBy = TestUserId
            };
            db.TrackingCycles.Add(cycle);
            await db.SaveChangesAsync(cancellationToken);

            if (existingReminder == null)
            {
                db.MaintenanceReminders.Add(new MaintenanceReminder
                {
                    Id = expectedReminderId,
                    TrackingCycleId = cycle.Id,
                    CurrentOdometer = reminderCurrentOdo,
                    TargetOdometer = predictedNextOdo,
                    TargetDate = predictedNextDate,
                    Level = level,
                    PercentageRemaining = pct,
                    IsNotified = false,
                    NotifiedDate = null,
                    Status = ReminderStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = TestUserId
                });
            }
            else
            {
                existingReminder.TrackingCycleId = cycle.Id;
                existingReminder.CurrentOdometer = reminderCurrentOdo;
                existingReminder.TargetOdometer = predictedNextOdo;
                existingReminder.TargetDate = predictedNextDate;
                existingReminder.Level = level;
                existingReminder.PercentageRemaining = pct;
                existingReminder.Status = ReminderStatus.Active;
                existingReminder.IsNotified = false;
                existingReminder.NotifiedDate = null;
            }

            await db.SaveChangesAsync(cancellationToken);
            addedOrRepaired++;
            logger?.LogInformation(
                "Ensured active TrackingCycle + MaintenanceReminder (level {Level}) for test vehicle {UserVehicleId}",
                level,
                userVehicleId);
        }

        if (addedOrRepaired > 0)
            logger?.LogInformation(
                "Added or repaired {Count} declared maintenance chains (PartTracking + TrackingCycle + reminder) for test vehicle {UserVehicleId}",
                addedOrRepaired,
                userVehicleId);

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

                CreatedAt = DateTime.UtcNow,
                CreatedBy = TestUserId
            };
            db.VehiclePartTrackings.Add(partTracking);
            await db.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Added undeclared part tracking (IsDeclared=false) for test vehicle {UserVehicleId}, PartCategoryId {PartCategoryId}", userVehicleId, partCategoryId);
        }

        await EnsureDemoMaintenanceRecordsAsync(db, userVehicleId, logger, cancellationToken);
    }
}
