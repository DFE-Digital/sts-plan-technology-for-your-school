using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class RecommendationEntityConfiguration : IEntityTypeConfiguration<RecommendationEntity>
{
    public void Configure(EntityTypeBuilder<RecommendationEntity> builder)
    {
        builder.ToTable("recommendation", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ContentfulRef).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DateCreated).HasDefaultValue();
        builder
            .Property(x => x.RecommendationText)
            .HasMaxLength(4000) // NVARCHAR max length
            .IsRequired();
        builder.Property(x => x.QuestionId).IsRequired();
        builder.Property(x => x.Archived).IsRequired();
        builder.Property(x => x.QuestionContentfulRef).IsRequired(false);
        builder.Property(b => b.UserActionId).IsRequired(false);

        builder
            .HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(r => r.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
