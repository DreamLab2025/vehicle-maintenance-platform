using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Verendar.Ai.Domain.Entities;

namespace Verendar.Ai.Infrastructure.Configurations;

public class AiPromptHistoryConfiguration : IEntityTypeConfiguration<AiPromptHistory>
{
    public void Configure(EntityTypeBuilder<AiPromptHistory> builder)
    {
        builder.ToTable("AiPromptHistories");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.AiPromptId, e.VersionNumber }).IsUnique();

        builder.Property(e => e.Content).IsRequired().HasColumnType("text");
        builder.Property(e => e.Note).HasMaxLength(500);
    }
}
