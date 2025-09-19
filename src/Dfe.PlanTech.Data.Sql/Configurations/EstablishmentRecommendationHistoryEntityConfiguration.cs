using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

[ExcludeFromCodeCoverage]
internal class EstablishmentRecommendationHistoryEntityConfiguration : IEntityTypeConfiguration<EstablishmentRecommendationHistoryEntity>
{
    public void Configure(EntityTypeBuilder<EstablishmentRecommendationHistoryEntity> builder)
    {
        builder.HasKey(history => new { history.EstablishmentId, history.RecommendationId });

        builder.Property(history => history.DateCreated).HasColumnType("datetime").HasDefaultValue();
        builder.Property(history => history.PreviousStatus).HasMaxLength(50);
        builder.Property(history => history.NewStatus).HasMaxLength(50);
        builder.Property(history => history.NoteText).HasMaxLength(4000); // NVARCHAR max length
    }
}
