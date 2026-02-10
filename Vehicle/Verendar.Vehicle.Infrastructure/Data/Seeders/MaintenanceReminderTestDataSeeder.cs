using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Verendar.Common.Databases.Base;
using Verendar.Vehicle.Domain.Entities;
using Verendar.Vehicle.Domain.Enums;
using Verendar.Vehicle.Infrastructure.Data;

namespace Verendar.Vehicle.Infrastructure.Seeders;

/// <summary>
/// Seed test user vehicle + urgent maintenance reminder for background job flows:
/// - MaintenanceReminderJob (Urgent reminder → publish MaintenanceReminderEvent)
/// - OdometerReminderJob (stale odometer → publish OdometerReminderEvent)
/// </summary>
public static class MaintenanceReminderTestDataSeeder
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid WaveAlphaRedVariantId = Guid.Parse("e0000001-0000-0000-0000-000000000001");
    private static readonly Guid EngineOilCategoryId = Guid.Parse("c0000001-0000-0000-0000-000000000001");

    public static async Task SeedAsync(VehicleDbContext db, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var userId = TestUserId;

        var existingVehicle = await db.UserVehicles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

        if (existingVehicle != null)
        {
            var hasUrgent = await db.MaintenanceReminders
                .IgnoreQueryFilters()
                .Include(m => m.PartTracking)
                .AnyAsync(m => m.PartTracking.UserVehicleId == existingVehicle.Id && m.Level == ReminderLevel.Urgent, cancellationToken);
            if (!hasUrgent)
            {
                await EnsureUrgentReminderForVehicleAsync(db, existingVehicle.Id, logger, cancellationToken);
            }
            logger?.LogDebug("Test user vehicle already exists for user {UserId}", userId);
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

        var partTracking = new VehiclePartTracking
        {
            Id = Guid.CreateVersion7(),
            UserVehicleId = userVehicle.Id,
            PartCategoryId = EngineOilCategoryId,
            LastReplacementOdometer = 2000,
            LastReplacementDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
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
            CurrentOdometer = 5000,
            TargetOdometer = 6000,
            Level = ReminderLevel.Urgent,
            PercentageRemaining = 5m,
            IsNotified = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.MaintenanceReminders.Add(reminder);
        await db.SaveChangesAsync(cancellationToken);

        logger?.LogInformation(
            "Seeded test user vehicle (UserId: {UserId}, UserVehicleId: {UserVehicleId}) with Urgent reminder for MaintenanceReminderJob and stale odometer for OdometerReminderJob",
            userId, userVehicle.Id);
    }

    private static async Task EnsureUrgentReminderForVehicleAsync(VehicleDbContext db, Guid userVehicleId, ILogger? logger, CancellationToken cancellationToken)
    {
        var partTracking = await db.VehiclePartTrackings
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(pt => pt.UserVehicleId == userVehicleId && pt.PartCategoryId == EngineOilCategoryId, cancellationToken);
        if (partTracking == null)
        {
            partTracking = new VehiclePartTracking
            {
                Id = Guid.CreateVersion7(),
                UserVehicleId = userVehicleId,
                PartCategoryId = EngineOilCategoryId,
                LastReplacementOdometer = 2000,
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
            CurrentOdometer = 5000,
            TargetOdometer = 6000,
            Level = ReminderLevel.Urgent,
            PercentageRemaining = 5m,
            IsNotified = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.MaintenanceReminders.Add(reminder);
        await db.SaveChangesAsync(cancellationToken);
        logger?.LogInformation("Added Urgent MaintenanceReminder for test vehicle {UserVehicleId}", userVehicleId);
    }
}
