using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationChunkEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationChunkDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationChunkDbEntity> builder)
    {
        builder.HasMany(chunk => chunk.Answers)
              .WithMany(content => content.RecommendationChunks)
              .UsingEntity<RecommendationChunkAnswerDbEntity>();

        builder.HasMany(chunk => chunk.Content)
              .WithMany(content => content.RecommendationChunk)
              .UsingEntity<RecommendationChunkContentDbEntity>();
    }
}
