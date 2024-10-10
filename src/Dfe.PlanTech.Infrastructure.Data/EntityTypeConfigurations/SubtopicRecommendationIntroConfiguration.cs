using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class SubtopicRecommendationIntroConfiguration : IEntityTypeConfiguration<SubtopicRecommendationIntroDbEntity>
{
    public void Configure(EntityTypeBuilder<SubtopicRecommendationIntroDbEntity> builder)
    {
        builder.ToTable("SubtopicRecommendationIntros", CmsDbContext.Schema);
    }
}
