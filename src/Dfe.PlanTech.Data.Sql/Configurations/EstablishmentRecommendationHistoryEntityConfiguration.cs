using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

/// Created via: src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2025/20250905_1200_AddRecommendationTables.sql
/// Updated via: src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2025/20250920_1200_FixRecommendationHistoryForAppendOnly.sql
internal class EstablishmentRecommendationHistoryEntityConfiguration
    : IEntityTypeConfiguration<EstablishmentRecommendationHistoryEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentRecommendationHistoryEntity> builder)
    {
        // Use identity column as primary key (changed from composite key)
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).ValueGeneratedOnAdd();

        builder
            .Property(history => history.DateCreated)
            .HasColumnType("datetime")
            .HasDefaultValue();
        builder.Property(history => history.PreviousStatus).HasMaxLength(50);
        builder.Property(history => history.NewStatus).HasMaxLength(50);
        builder.Property(history => history.NoteText).HasMaxLength(4000); // NVARCHAR max length

        // Create index on the composite key for performance (matches migration script)
        builder
            .HasIndex(h => new { h.EstablishmentId, h.RecommendationId })
            .HasDatabaseName("IX_establishmentRecommendationHistory_EstablishmentRecommendation");

        // Create index on dateCreated for ordering (matches migration script)
        builder
            .HasIndex(h => h.DateCreated)
            .HasDatabaseName("IX_establishmentRecommendationHistory_DateCreated")
            .IsDescending();

        builder
            .HasOne(h => h.Establishment)
            .WithMany()
            .HasForeignKey(h => h.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(h => h.MatEstablishment)
            .WithMany()
            .HasForeignKey(h => h.MatEstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(h => h.Recommendation)
            .WithMany()
            .HasForeignKey(h => h.RecommendationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
