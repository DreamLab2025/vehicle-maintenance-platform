using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Verendar.Ai.Domain.Entities;

namespace Verendar.Ai.Infrastructure.Configurations;

public class AiPromptConfiguration : IEntityTypeConfiguration<AiPrompt>
{
    public void Configure(EntityTypeBuilder<AiPrompt> builder)
    {
        builder.ToTable("AiPrompts");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Operation).IsUnique();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Content).IsRequired().HasColumnType("text");
        builder.Property(e => e.VersionNumber).HasDefaultValue(1);

        builder.HasMany(e => e.Histories)
            .WithOne(h => h.AiPrompt)
            .HasForeignKey(h => h.AiPromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
