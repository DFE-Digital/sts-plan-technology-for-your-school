using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationChunkContentConfiguration : IEntityTypeConfiguration<RecommendationChunkContentDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationChunkContentDbEntity> builder)
    {
        builder.ToTable("RecommendationChunkContents", CmsDbContext.Schema);
    }
}
