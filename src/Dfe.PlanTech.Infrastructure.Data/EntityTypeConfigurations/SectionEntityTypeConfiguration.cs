using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.PlanTech.Infrastructure.Data.EntityTypeConfigurations;

[ExcludeFromCodeCoverage]
public class SectionEntityTypeConfiguration : IEntityTypeConfiguration<SectionDbEntity>
{
    public void Configure(EntityTypeBuilder<SectionDbEntity> builder)
    {
        builder.HasOne(section => section.InterstitialPage)
                .WithOne(page => page.Section)
                .HasForeignKey<SectionDbEntity>()
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(section => section.Category)
                .WithMany(category => category.Sections)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(section => section.Questions)
                .WithOne(question => question.Section)
                .OnDelete(DeleteBehavior.Restrict);
    }
}
