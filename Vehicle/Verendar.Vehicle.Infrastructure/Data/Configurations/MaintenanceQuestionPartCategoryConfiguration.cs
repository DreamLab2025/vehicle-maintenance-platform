namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceQuestionPartCategoryConfiguration : IEntityTypeConfiguration<MaintenanceQuestionPartCategory>
    {
        public void Configure(EntityTypeBuilder<MaintenanceQuestionPartCategory> builder)
        {
            builder.HasIndex(e => new { e.MaintenanceQuestionId, e.PartCategoryId })
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasOne(x => x.MaintenanceQuestion)
                .WithMany(q => q.PartCategoryLinks)
                .HasForeignKey(x => x.MaintenanceQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PartCategory)
                .WithMany(p => p.MaintenanceQuestionLinks)
                .HasForeignKey(x => x.PartCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
