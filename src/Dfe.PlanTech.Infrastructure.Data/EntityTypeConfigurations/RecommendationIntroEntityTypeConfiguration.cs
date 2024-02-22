using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

public class RecommendationIntroEntityTypeConfiguration : IEntityTypeConfiguration<RecommendationIntroDbEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationIntroDbEntity> builder)
    {
        builder.HasMany(intro => intro.Content)
            .WithMany()
            .UsingEntity(join =>
            {
                join.ToTable("RecommendationIntroContents");
                join.HasOne(typeof(RecommendationIntroDbEntity)).WithMany().HasForeignKey("RecommendationIntroId").HasPrincipalKey("Id");
                join.HasOne(typeof(ContentComponentDbEntity)).WithMany().HasForeignKey("ContentComponentId").HasPrincipalKey("Id");
                join.Property<long>("Id");
                join.HasIndex("Id");
            });
    }
}
