using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class SubtopicRecommendationEntityTypeConfiguration : IEntityTypeConfiguration<SubTopicRecommendationDbEntity>
{
    public void Configure(EntityTypeBuilder<SubTopicRecommendationDbEntity> builder)
    {
        builder.HasMany(subtopicRecommendation => subtopicRecommendation.Intros)
              .WithMany()
              .UsingEntity(join =>
              {
                  join.ToTable("SubtopicRecommendationIntros");
                  join.HasOne(typeof(SubTopicRecommendationDbEntity)).WithMany().HasForeignKey("SubtopicRecommendationId").HasPrincipalKey("Id");
                  join.HasOne(typeof(RecommendationIntroDbEntity)).WithMany().HasForeignKey("RecommendationIntroId").HasPrincipalKey("Id");
                  join.Property<long>("Id");
                  join.HasIndex("Id");
              });
    }
}
