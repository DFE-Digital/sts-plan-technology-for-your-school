using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

internal class QuestionEntityConfiguration : IEntityTypeConfiguration<QuestionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("question", "dbo");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder
            .Property(x => x.QuestionText)
            .HasMaxLength(4000) // NVARCHAR max length
            .IsRequired(false);
        builder.Property(x => x.ContentfulRef).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DateCreated).ValueGeneratedOnAdd();
        builder.Property(x => x.Order).IsRequired(false);
        builder.Property(x => x.UserActionId).IsRequired(false);
    }
}
