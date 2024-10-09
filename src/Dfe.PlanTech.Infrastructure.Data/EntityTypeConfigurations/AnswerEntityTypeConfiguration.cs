using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class AnswerEntityTypeConfiguration : IEntityTypeConfiguration<AnswerDbEntity>
{
    public void Configure(EntityTypeBuilder<AnswerDbEntity> builder)
    {
        builder.HasOne(a => a.NextQuestion).WithMany(q => q.PreviousAnswers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.ParentQuestion).WithMany(q => q.Answers).OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Answers", CmsDbContext.Schema);
    }
}
