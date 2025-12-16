using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class RecommendationEntityConfiguration : IEntityTypeConfiguration<RecommendationEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationEntity> builder)
    {
        builder.HasKey(recommendation => recommendation.Id);
        builder.Property(recommendation => recommendation.Id).ValueGeneratedOnAdd();
        builder.Property(recommendation => recommendation.RecommendationText).HasMaxLength(4000); // NVARCHAR max length
        builder.Property(recommendation => recommendation.ContentfulRef).HasMaxLength(50);
        builder.Property(recommendation => recommendation.DateCreated).HasColumnType("datetime").HasDefaultValue();

        builder.HasOne(recommendation => recommendation.Question)
            .WithMany()
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
