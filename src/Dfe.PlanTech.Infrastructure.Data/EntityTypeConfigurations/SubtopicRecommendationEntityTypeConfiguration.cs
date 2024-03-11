using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class SubtopicRecommendationEntityTypeConfiguration : IEntityTypeConfiguration<SubtopicRecommendationDbEntity>
{
    public void Configure(EntityTypeBuilder<SubtopicRecommendationDbEntity> builder)
    {
        builder.HasMany(subtopicRecommendation => subtopicRecommendation.Intros)
              .WithMany();
    }
}
