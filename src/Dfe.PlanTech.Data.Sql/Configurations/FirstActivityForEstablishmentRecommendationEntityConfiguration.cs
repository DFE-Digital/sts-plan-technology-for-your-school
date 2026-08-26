using Dfe.PlanTech.Data.Sql.Common;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations
{
    internal class FirstActivityForEstablishmentRecommendationEntityConfiguration
        : IEntityTypeConfiguration<FirstActivityForEstablishmentRecommendationEntity>
    {
        public void Configure(
            EntityTypeBuilder<FirstActivityForEstablishmentRecommendationEntity> builder
        )
        {
            builder.HasNoKey();

            builder.Property(x => x.StatusChangeDate);
            builder
                .Property(x => x.Status)
                .HasColumnName("statusText")
                .HasConversion(StatusConverters.RecommendationStatusConverter)
                .HasMaxLength(50);
            builder.Property(x => x.SchoolName).HasMaxLength(200);
            builder.Property(x => x.GroupName).HasMaxLength(200);
            builder.Property(x => x.UserId);
            builder.Property(x => x.QuestionText).HasMaxLength(4000);
            builder.Property(x => x.AnswerText).HasMaxLength(4000);
        }
    }
}
