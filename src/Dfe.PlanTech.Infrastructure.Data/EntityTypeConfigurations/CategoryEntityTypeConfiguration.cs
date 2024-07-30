using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<CategoryDbEntity>
{
    public void Configure(EntityTypeBuilder<CategoryDbEntity> builder)
    {
        builder.HasMany(category => category.Sections)
                .WithOne(section => section.Category)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(category => category.Header).AutoInclude();

        builder.Navigation(category => category.Sections).AutoInclude();
    }
}
