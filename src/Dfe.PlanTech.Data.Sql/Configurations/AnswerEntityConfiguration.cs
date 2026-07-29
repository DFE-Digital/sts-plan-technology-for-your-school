using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class AnswerEntityConfiguration : IEntityTypeConfiguration<AnswerEntity>
{
    public void Configure(EntityTypeBuilder<AnswerEntity> builder)
    {
        builder.ToTable("answer");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.AnswerText).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ContentfulRef).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DateCreated).ValueGeneratedOnAdd();
        builder.Property(x => x.UserActionId).IsRequired(false);
    }
}
