using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationIntroContentEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationIntroContentDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationIntroContentDbEntity> builder)
    {
        builder.HasOne(pc => pc.ContentComponent).WithMany(c => c.RecommendationIntroContentJoins);
    }
}
