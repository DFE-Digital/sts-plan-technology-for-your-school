using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationSectionConfiguration : IEntityTypeConfiguration<RecommendationSectionDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationSectionDbEntity> builder)
    {
        builder.ToTable("RecommendationSections", CmsDbContext.Schema);
    }
}
