using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationIntroEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationIntroDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationIntroDbEntity> builder)
    {
        builder.HasMany(intro => intro.Content)
                .WithMany(content => content.RecommendationIntro)
                .UsingEntity<RecommendationIntroContentDbEntity>();
    }
}
