using Dfe.PlanTech.Data.Sql.Common;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class EstablishmentRecommendationHistoryEntityConfiguration
    : IEntityTypeConfiguration<EstablishmentRecommendationHistoryEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentRecommendationHistoryEntity> builder)
    {
        builder.ToTable("establishmentRecommendationHistory", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.DateCreated).HasDefaultValue();
        builder.Property(x => x.EstablishmentId).IsRequired();
        builder.Property(x => x.MatEstablishmentId).IsRequired(false);
        builder.Property(x => x.RecommendationId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder
            .Property(x => x.PreviousStatus)
            .HasMaxLength(100)
            .HasConversion(StatusConverters.RecommendationStatusConverter)
            .IsRequired(false);
        builder
            .Property(x => x.NewStatus)
            .HasConversion(StatusConverters.RecommendationStatusConverter)
            .HasMaxLength(100);
        builder.Property(x => x.NoteText).HasMaxLength(4000).IsRequired(false);
        builder.Property(x => x.ResponseId).IsRequired(false);
        builder.Property(x => x.UserActionId).IsRequired(false);

        builder
            .HasIndex(x => new { x.EstablishmentId, x.RecommendationId })
            .HasDatabaseName("IX_establishmentRecommendationHistory_EstablishmentRecommendation");

        builder
            .HasIndex(x => x.DateCreated)
            .HasDatabaseName("IX_establishmentRecommendationHistory_DateCreated")
            .IsDescending();

        builder
            .HasIndex(x => x.ResponseId)
            .HasDatabaseName("IX_establishmentRecommendationHistory_responseId");

        builder
            .HasOne(x => x.Establishment)
            .WithMany()
            .HasForeignKey(x => x.EstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.MatEstablishment)
            .WithMany()
            .HasForeignKey(x => x.MatEstablishmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Recommendation)
            .WithMany()
            .HasForeignKey(x => x.RecommendationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Response)
            .WithMany()
            .HasForeignKey(x => x.ResponseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
