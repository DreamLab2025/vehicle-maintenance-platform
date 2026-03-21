namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceReminderConfiguration : IEntityTypeConfiguration<MaintenanceReminder>
    {
        public void Configure(EntityTypeBuilder<MaintenanceReminder> builder)
        {
            builder.HasIndex(e => new { e.TrackingCycleId, e.Level }).HasFilter("\"DeletedAt\" IS NULL");

            builder.HasOne(r => r.TrackingCycle)
                .WithMany(c => c.Reminders)
                .HasForeignKey(r => r.TrackingCycleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.PercentageRemaining).HasColumnType("decimal(5,2)");
        }
    }
}
