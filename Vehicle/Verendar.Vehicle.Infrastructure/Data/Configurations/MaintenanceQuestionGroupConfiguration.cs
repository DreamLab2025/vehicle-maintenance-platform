namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceQuestionGroupConfiguration : IEntityTypeConfiguration<MaintenanceQuestionGroup>
    {
        public void Configure(EntityTypeBuilder<MaintenanceQuestionGroup> builder)
        {
            builder.HasIndex(e => e.Code).IsUnique().HasFilter("\"DeletedAt\" IS NULL");
            builder.HasIndex(e => e.DisplayOrder).HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
