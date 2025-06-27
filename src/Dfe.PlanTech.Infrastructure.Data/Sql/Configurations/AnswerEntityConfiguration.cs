using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Configurations;

internal class AnswerEntityConfiguration : IEntityTypeConfiguration<AnswerEntity>
{
    public void Configure(EntityTypeBuilder<AnswerEntity> builder)
    {
        builder.ToTable("answer");
        builder.HasKey(answer => answer.Id);
        builder.Property(answer => answer.Id).ValueGeneratedOnAdd();
        builder.Property(answer => answer.AnswerText).HasMaxLength(4000); // NVARCHAR Max Length
        builder.Property(answer => answer.ContentfulSysId).HasMaxLength(50);
        builder.Property(answer => answer.DateCreated).ValueGeneratedOnAdd();
    }
}
