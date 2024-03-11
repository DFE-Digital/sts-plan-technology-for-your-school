using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class RecommendationChunkEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationChunkDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationChunkDbEntity> builder)
    {
        builder.HasMany(chunk => chunk.Content)
              .WithMany()
              .UsingEntity(join =>
              {
                  join.ToTable("RecommendationChunkContents");
                  join.HasOne(typeof(RecommendationChunkDbEntity)).WithMany().HasForeignKey("RecommendationChunkId").HasPrincipalKey("Id");
                  join.HasOne(typeof(ContentComponentDbEntity)).WithMany().HasForeignKey("ContentComponentId").HasPrincipalKey("Id");
                  join.Property<long>("Id");
                  join.HasIndex("Id");
              });

        builder.HasMany(chunk => chunk.Answers)
              .WithMany()
              .UsingEntity(join =>
              {
                  join.ToTable("RecommendationChunkAnswers");
                  join.HasOne(typeof(RecommendationChunkDbEntity)).WithMany().HasForeignKey("RecommendationChunkId").HasPrincipalKey("Id");
                  join.HasOne(typeof(AnswerDbEntity)).WithMany().HasForeignKey("AnswerId").HasPrincipalKey("Id");
                  join.Property<long>("Id");
                  join.HasIndex("Id");
              });
    }
}
