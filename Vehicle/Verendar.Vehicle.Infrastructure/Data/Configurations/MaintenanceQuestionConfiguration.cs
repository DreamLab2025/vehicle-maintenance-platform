namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceQuestionConfiguration : IEntityTypeConfiguration<MaintenanceQuestion>
    {
        public void Configure(EntityTypeBuilder<MaintenanceQuestion> builder)
        {
            builder.HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            builder.HasIndex(e => e.GroupId).HasFilter("\"DeletedAt\" IS NULL");

            builder.HasOne(q => q.Group)
                .WithMany(g => g.Questions)
                .HasForeignKey(q => q.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
