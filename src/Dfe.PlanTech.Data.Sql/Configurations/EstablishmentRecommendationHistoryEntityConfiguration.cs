using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class EstablishmentRecommendationHistoryEntityConfiguration
    : IEntityTypeConfiguration<EstablishmentRecommendationHistoryEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentRecommendationHistoryEntity> builder)
    {
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).ValueGeneratedOnAdd();

        builder
            .Property(history => history.DateCreated)
            .HasColumnType("datetime")
            .HasDefaultValue();

        builder.Property(history => history.PreviousStatus).HasMaxLength(50);
        builder.Property(history => history.NewStatus).HasMaxLength(50);
        builder.Property(history => history.NoteText);

        builder.Property(h => h.ResponseId)
            .HasColumnName("responseId");

        builder
            .HasIndex(h => new { h.EstablishmentId, h.RecommendationId })
            .HasDatabaseName("IX_establishmentRecommendationHistory_EstablishmentRecommendation");

        builder
            .HasIndex(h => h.DateCreated)
            .HasDatabaseName("IX_establishmentRecommendationHistory_DateCreated")
            .IsDescending();

        builder
            .HasIndex(h => h.ResponseId)
            .HasDatabaseName("IX_establishmentRecommendationHistory_responseId");

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

        builder
            .HasOne(h => h.Response)
            .WithMany()
            .HasForeignKey(h => h.ResponseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
                                               
    }
}
