using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class RecommendationSectionEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationSectionDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationSectionDbEntity> builder)
    {
        builder.HasMany(section => section.Chunks)
        .WithMany(chunk => chunk.RecommendationSections)
        .UsingEntity(join =>
        {
            join.ToTable("RecommendationSectionChunks");
            join.HasOne(typeof(RecommendationSectionDbEntity)).WithMany().HasForeignKey("RecommendationSectionId").HasPrincipalKey("Id");
            join.HasOne(typeof(RecommendationChunkDbEntity)).WithMany().HasForeignKey("RecommendationChunkId").HasPrincipalKey("Id");
            join.Property<long>("Id");
            join.HasIndex("Id");
        });

        builder.HasMany(section => section.Answers)
            .WithMany()
            .UsingEntity(join =>
            {
                join.ToTable("RecommendationSectionAnswers");
                join.HasOne(typeof(RecommendationSectionDbEntity)).WithMany().HasForeignKey("RecommendationSectionId").HasPrincipalKey("Id");
                join.HasOne(typeof(AnswerDbEntity)).WithMany().HasForeignKey("AnswerId").HasPrincipalKey("Id");
                join.Property<long>("Id");
                join.HasIndex("Id");
            });
    }
}
