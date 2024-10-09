using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class QuestionEntityTypeConfiguration : IEntityTypeConfiguration<QuestionDbEntity>
{
    public void Configure(EntityTypeBuilder<QuestionDbEntity> builder)
    {
        builder.ToTable("Questions", CmsDbContext.Schema);
    }
}
