namespace Verendar.Vehicle.Infrastructure.Data.Configurations
{
    public class MaintenanceQuestionOptionConfiguration : IEntityTypeConfiguration<MaintenanceQuestionOption>
    {
        public void Configure(EntityTypeBuilder<MaintenanceQuestionOption> builder)
        {
            builder.HasIndex(e => new { e.QuestionId, e.OptionKey })
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");

            builder.HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
