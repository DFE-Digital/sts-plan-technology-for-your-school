using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Data.Sql.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Data.Sql.Configurations;

[ExcludeFromCodeCoverage]
internal class QuestionEntityConfiguration : IEntityTypeConfiguration<QuestionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("question");
        builder.HasKey(question => question.Id);
        builder.Property(question => question.Id).ValueGeneratedOnAdd();
        builder.Property(question => question.QuestionText).HasMaxLength(4000); // NVARCHAR max length
        builder.Property(question => question.ContentfulRef).HasMaxLength(50);
        builder.Property(question => question.DateCreated).ValueGeneratedOnAdd();
    }
}
