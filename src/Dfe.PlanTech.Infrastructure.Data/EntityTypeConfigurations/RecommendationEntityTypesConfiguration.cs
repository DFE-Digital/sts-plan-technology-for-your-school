using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationChunkContentEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationChunkContentDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationChunkContentDbEntity> builder)
    {
        builder.HasOne(pc => pc.ContentComponent).WithMany(c => c.RecommendationChunkContentJoins);
    }
}
