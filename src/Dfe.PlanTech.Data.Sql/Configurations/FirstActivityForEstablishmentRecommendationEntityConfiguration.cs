using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            builder.Property(e => e.StatusChangeDate).HasColumnType("datetime");
            builder.Property(e => e.StatusText).HasMaxLength(50);
            builder.Property(e => e.SchoolName).HasMaxLength(200);
            builder.Property(e => e.GroupName).HasMaxLength(200);
            builder.Property(e => e.UserId).HasColumnType("int");
            builder.Property(e => e.QuestionText).HasMaxLength(4000);
            builder.Property(e => e.AnswerText).HasMaxLength(4000);
        }
    }
}
