using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationSectionEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationSectionDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationSectionDbEntity> builder)
    {
        builder.HasMany(section => section.Chunks)
        .WithMany(chunk => chunk.RecommendationSections);
    }
}
